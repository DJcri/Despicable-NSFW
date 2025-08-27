using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Despicable;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class PawnRenderNodeWorker_GraphicVariants : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms)) return false;

            if (parms.Portrait) return false;
            //don't draw if not visible at tick
            if (node.AnimationWorker is AnimationWorker_ExtendedKeyframes extendedAnimator)
            {
                AnimationPart animPart;
                node.tree.TryGetAnimationPartForNode(node, out animPart);
                return extendedAnimator.VisibleAtTick(node.tree.AnimationTick, animPart);
            }

            //don't draw at all if not animating
            return false;
        }
        protected override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
        {
            //if node is animating, and is a graphic variant type of node
            //and node is one with graphic variants
            //and texpathvariant is set
            AnimationPart animPart;
            node.tree.TryGetAnimationPartForNode(node, out animPart);

            if ((node.AnimationWorker is AnimationWorker_ExtendedKeyframes extendedAnimWorker)
                && (node is PawnRenderNode_GraphicVariants nodeWithGraphicVariants)
                && extendedAnimWorker.VariantTexPathOnTick(node.tree.AnimationTick, animPart) != null)
            {
                Material materialVariant = GetMaterialVariant(nodeWithGraphicVariants, parms, (int)extendedAnimWorker.VariantTexPathOnTick(node.tree.AnimationTick, animPart));
                
                if (materialVariant != null) {
                    return materialVariant;
                }
            }

            //otherwise return original texture
            return base.GetMaterial(node, parms);
        }

        public virtual Material GetMaterialVariant(PawnRenderNode_GraphicVariants node, PawnDrawParms parms, int variant)
        {
            Material material = node.getGraphicVariant(variant)?.NodeGetMat(parms);

            if (material == null) return null;

            if (!parms.Portrait && parms.flags.FlagSet(PawnRenderFlags.Invisible))
            {
                material = InvisibilityMatPool.GetInvisibleMat(material);
            }

            return material;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 regularOffsets = base.OffsetFor(node, parms, out pivot);

            if ((node.Props as PawnRenderNodeProperties_GraphicVariants)?.propOffsetDef?.offsets is List<BaseAnimationOffset> offsets)
            {
                foreach (BaseAnimationOffset offset in offsets)
                {
                    if (offset.appliesToPawn(node.tree.pawn))
                    {
                        //modify offset of prop for animationOffset position
                        regularOffsets += offset.getOffset(node.tree.pawn) ?? Vector3.zero;
                        return regularOffsets;
                    }
                }
            }

            //unmodified; no offsets found
            return regularOffsets;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            Vector3 regularScale = base.ScaleFor(node, parms);

            if ((node.Props as PawnRenderNodeProperties_GraphicVariants)?.propOffsetDef?.offsets is List<BaseAnimationOffset> offsets)
            {
                foreach (BaseAnimationOffset offset in offsets)
                {

                    if (offset.appliesToPawn(node.tree.pawn))
                    {

                        //modify scale of prop for animationOffset position
                        regularScale = regularScale.MultipliedBy(offset.getScale(node.tree.pawn) ?? Vector3.one);
                        return regularScale;

                    }
                }
            }

            return regularScale;
        }

        public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
        {
            Quaternion rotation = base.RotationFor(node, parms);

            if ((node.Props as PawnRenderNodeProperties_GraphicVariants)?.propOffsetDef?.offsets is List<BaseAnimationOffset> offsets)
            {
                foreach (BaseAnimationOffset offset in offsets)
                {
                    if (offset.appliesToPawn(node.tree.pawn))
                    {
                        //modify offset of prop for animationOffset rotation
                        rotation *= Quaternion.AngleAxis(offset.getRotation(node.tree.pawn) ?? 0, Vector3.up);
                        return rotation;

                    }
                }
            }

            //unmodified; no rotation offsets found
            return rotation;
        }
    }
}
