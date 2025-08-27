using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    // Head Rotation Code - Textures
    // it's fine to just edit each AppendRequests individually
    // because they all the parms are passed down to each child node recursively
    [HarmonyPatch(typeof(PawnRenderNode), "AppendRequests")]
    public static class HarmonyPatch_PawnRenderNode
    {
        //if rendernodetag is head, update PawnDrawParms so that head, and all children, are rotated for anim
        public static bool Prefix(ref PawnRenderNode __instance, ref PawnDrawParms parms)
        {
            if (__instance.AnimationWorker is AnimationWorker_ExtendedKeyframes extendedAnimWorker)
            {
                if (parms.Portrait) return true;
                AnimationPart animPart;
                __instance.tree.TryGetAnimationPartForNode(__instance, out animPart);

                // ADJUST FACING get rotated textures
                // compare the previous tick to the current tick; if the current tick rotation is different, recache
                if (animPart != null)
                {
                    parms.facing = extendedAnimWorker.FacingAtTick(__instance.tree.AnimationTick, animPart);
                }


                //INVIS IF ANIM CALLS FOR IT
                //replace maybe?

                //cheaper call now comparing prev tick to cur tick

                //not necessary because of new rendernodeworker hiding props now
                //nvm, keep it because you can hide head and body too, if need be
                return extendedAnimWorker.VisibleAtTick(__instance.tree.AnimationTick, animPart);
            }

            return true;
        }
    }
}
