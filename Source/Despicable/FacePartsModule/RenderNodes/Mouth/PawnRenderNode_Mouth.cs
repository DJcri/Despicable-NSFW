using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Despicable
{
    public class PawnRenderNode_Mouth : PawnRenderNode
    {
        CompFaceParts compFaceParts;

        public PawnRenderNode_Mouth(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
            compFaceParts = pawn.TryGetComp<CompFaceParts>();
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

            if (compFaceParts != null)
            {
                ExpressionDef baseExpression = compFaceParts?.baseExpression;
                ExpressionDef animExpression = compFaceParts?.animExpression;

                if (!(animExpression?.texPathMouth).NullOrEmpty())
                    this.props.texPath = animExpression.texPathMouth;
                else if (!(baseExpression?.texPathMouth).NullOrEmpty())
                    this.props.texPath = baseExpression.texPathMouth;
                else if (compFaceParts.mouthStyleDef != null && !compFaceParts.mouthStyleDef.texPath.NullOrEmpty())
                    this.props.texPath = compFaceParts.mouthStyleDef.texPath;
            }

            if (this.props.texPath.NullOrEmpty())
                this.props.texPath = "FaceParts/Details/detail_empty";

            return GraphicDatabase.Get<Graphic_Multi>(this.props.texPath, ShaderDatabase.Cutout, Vector2.one, ColorFor(pawn));
        }
    }
}