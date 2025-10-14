using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Despicable
{
    public class PawnRenderNodeWorker_EyeAddon : PawnRenderNodeWorker_FacePart
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (base.CanDrawNow(node, parms))
            {
                // Don't render expression texPath if portrait, use face style instead
                Pawn pawn = node.tree.pawn;
                HeadTypeDef headTypeDef = pawn?.story?.headType;

                // Don't render right-counterpart when facing west, as the mirrored textures already flip automatically!
                if (node.Props.debugLabel.ToLower().EndsWith("_r") && !(parms.facing == Rot4.South))
                {
                    return false;
                }

                // Don't render face parts if facing north
                if (!(parms.facing != Rot4.North))
                {
                    return parms.flipHead;
                }

                // Check for eyes
                List<Gene> genesDefs = pawn.genes.GenesListForReading.Where(g =>
                {
                    if (g.def.defName.ToLower().StartsWith("eyes_"))
                    {
                        return true;
                    }
                    return false;
                }).ToList();
                // If any gene interferes with face part, don't render it
                if (genesDefs.Count > 0) return false;

                return true;
            }
            return false;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Pawn pawn = parms.pawn;
            HeadTypeDef headType = pawn.story.headType;
            Vector3 vector = base.OffsetFor(node, parms, out pivot);

            float eyeOffset = 0.13f;
            if (headType.eyeOffsetEastWest.HasValue)
                eyeOffset = headType?.eyeOffsetEastWest.Value.x ?? eyeOffset;

            Vector3 side = Vector3.zero;
            if (parms.facing == Rot4.East)
            {
                side = Vector3.right;
            }
            else if (parms.facing == Rot4.West)
            {
                side = Vector3.left;
            }

            // If flipped and facing south, this is a symmetrical part
            // Apply proper offset

            if (parms.facing == Rot4.South)
            {
                if (node.Props.flipGraphic)
                {
                    vector.x += 0.09f * (parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor ?? 1f);
                }
                else
                {
                    vector.x -= 0.09f * (parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor ?? 1f);
                }
                vector *= parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor ?? 1f;
                return vector;
            }

            vector += side * (eyeOffset * (parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor ?? 1f));
            vector *= parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor ?? 1f;
            return vector;
        }
    }
}
