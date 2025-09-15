using RimWorld;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class PawnRenderNode_Mouth : PawnRenderNode
    {
        public PawnRenderNode_Mouth(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override GraphicMeshSet MeshSetFor(Pawn pawn)
        {
            return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
        }

        public override Verse.Graphic GraphicFor(Pawn pawn)
        {
            if (!pawn.health.hediffSet.HasHead)
            {
                return null;
            }
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                return null;
            }
            
            return GraphicDatabase.Get<Graphic_Multi>(this.TexPathFor(pawn), ShaderDatabase.Cutout, Vector2.one, ColorFor(pawn));
        }
    }
}