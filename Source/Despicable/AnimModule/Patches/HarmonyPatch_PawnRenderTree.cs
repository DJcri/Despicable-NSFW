using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(PawnRenderTree), "TryGetMatrix")]
    public class HarmonyPatch_PawnRenderTree
    {
        public static bool Prefix(PawnRenderTree __instance, Dictionary<PawnRenderNodeTagDef, PawnRenderNode> ___nodesByTag, PawnRenderNode node, ref PawnDrawParms parms, ref Matrix4x4 matrix, ref bool __result)
        {
            /*
             * Facing offsets fix
             */
            //find lowest parent that is animating, or nothing if not animating

            //don't do anything if portrait
            if (parms.Portrait) return true;

            PawnRenderNode animatingNode = node;
            while (animatingNode != null
                && !(animatingNode.AnimationWorker is AnimationWorker_ExtendedKeyframes))
            {
                animatingNode = animatingNode.parent;
            }

            //if animating parent node found,
            if (animatingNode?.AnimationWorker is AnimationWorker_ExtendedKeyframes animatingNodeAnimationWorker)
            {
                //change parm to facing to animate correctly
                AnimationPart animPart;
                animatingNode.tree.TryGetAnimationPartForNode(animatingNode, out animPart);
                parms.facing = animatingNodeAnimationWorker.FacingAtTick(__instance.AnimationTick, animPart);
            }

            // Absolute transformation for prop nodes
            if (node.Props is PawnRenderNodeProperties_GraphicVariants graphicVariantProp
                && graphicVariantProp.absoluteTransform)
            {
                matrix = parms.matrix;

                //absolute transform -- just use the node's transform, not its ancestors
                node.GetTransform(parms, out Vector3 offset, out Vector3 pivot, out Quaternion quaternion, out Vector3 scale);
                 
                if (offset != Vector3.zero) matrix *= Matrix4x4.Translate(offset);
                if (pivot != Vector3.zero) matrix *= Matrix4x4.Translate(pivot);
                if (quaternion != Quaternion.identity) matrix *= Matrix4x4.Rotate(quaternion);
                if (scale != Vector3.one) matrix *= Matrix4x4.Scale(scale);
                if (pivot != Vector3.zero) matrix *= Matrix4x4.Translate(scale).inverse;

                float num = node.Worker.AltitudeFor(node, parms);
                if (num != 0f)
                {
                    matrix *= Matrix4x4.Translate(Vector3.up * num);
                }

                __result = true;
                return false;
            }

            return true;
        }
    }

    //recaching
    //done here because changing parms causes recaching anyway, so might as well do it here
    [HarmonyPatch(typeof(PawnRenderTree), "AdjustParms")]
    public class HarmonyPatch_PawnRenderTree2
    {
        public static void Prefix(PawnRenderTree __instance, ref PawnDrawParms parms)
        {
            int animationTick = __instance.AnimationTick;

            AnimationPart animPart;
            __instance.rootNode.tree.TryGetAnimationPartForNode(__instance.rootNode, out animPart);

            if (__instance.rootNode.AnimationWorker is AnimationWorker_ExtendedKeyframes rootAnimWorkerExtended)
            {
                //recache during facing turn
                if (rootAnimWorkerExtended.ShouldRecache(animationTick, animPart))
                {
                    __instance.rootNode.requestRecache = true;
                    return;
                }
            }

            foreach (PawnRenderNode node in __instance.rootNode.children)
            {
                __instance.rootNode.tree.TryGetAnimationPartForNode(node, out animPart);
                if (node.AnimationWorker is AnimationWorker_ExtendedKeyframes animWorkerExtended)
                {
                    //recache during flicker on/off
                    if (animWorkerExtended.ShouldRecache(animationTick, animPart))
                    {
                        node.requestRecache = true;
                        return;
                    }
                }
            }
        }
    }
}
