using Despicable;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Despicable
{
    public class PawnRenderNode_EyeAddon : PawnRenderNode
    {
        CompFaceParts compFaceParts;

        public PawnRenderNode_EyeAddon(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
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

                switch (this.props.debugLabel)
                {
                    case "FacePart_Eye_L":
                    case "FacePart_Eye_R":
                        if (animExpression?.texPathEyes != null)
                            this.props.texPath = FacePartsUtil.GetEyePath(pawn, animExpression?.texPathEyes);
                        else if (compFaceParts.baseExpression?.texPathEyes != null)
                            this.props.texPath = FacePartsUtil.GetEyePath(pawn, compFaceParts.baseExpression?.texPathEyes);
                        else if (compFaceParts.eyeStyleDef != null && !compFaceParts.eyeStyleDef.texPath.NullOrEmpty())
                            this.props.texPath = FacePartsUtil.GetEyePath(pawn, compFaceParts.eyeStyleDef?.texPath);
                        this.props.texPath = FacePartsUtil.GetEyePath(pawn, this.props.texPath); // Ensures it's a valid path
                        break;
                    case "FacePart_Detail_L":
                    case "FacePart_Detail_R":
                        //** Conditional rendering (drunk, high, tired, happy... etc.)
                        // Changes the base facial expression of pawn, not prioritized
                        // over animation expression
                        compFaceParts.baseExpression = null;

                        // If lovin' HIGHEST PRIORITY
                        if (LovinUtil.IsLovin(pawn))
                        {
                            if (PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Female))
                            {
                                compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Lovin;
                                this.props.texPath = "FaceParts/Details/detail_cheekblush";
                            }
                        }

                        // If asleep
                        if (compFaceParts.baseExpression == null && PawnStateUtil.IsAsleep(pawn))
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_EyesClosed;
                        }

                        // If berserk
                        if (compFaceParts.baseExpression == null && PawnStateUtil.isBerserk(pawn))
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Berserk;
                        }

                        // If in mental break
                        if (compFaceParts.baseExpression == null && PawnStateUtil.hasMentalBreak(pawn))
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Distressed;
                        }

                        // If baby or toddler
                        if (compFaceParts.baseExpression == null && PawnStateUtil.isInfant(pawn))
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Infant;
                        }

                        // If drunk
                        if (compFaceParts.baseExpression == null && PawnStateUtil.IsDrunk(pawn))
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Drunk;
                            this.props.texPath = "FaceParts/Details/detail_cheekblush";
                        }

                        // If tired
                        if (compFaceParts.baseExpression == null && PawnStateUtil.IsTired(pawn))
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Tired;
                            this.props.texPath = "FaceParts/Details/detail_darkcircles";
                        }

                        // If drafted, LOWEST PRIORITY
                        if (compFaceParts.baseExpression == null && pawn.Drafted)
                        {
                            compFaceParts.baseExpression = FacePartsModule_ExpressionDefOf.FacialExpression_Drafted;
                        }
                        break;
                    case "FacePart_SecondaryDetail_L":
                    case "FacePart_SecondaryDetail_R":
                        if (compFaceParts.animExpression != null)
                            this.props.texPath = animExpression?.texPathDetail ?? "FaceParts/Details/detail_empty";
                        break;
                }
            }

            if (this.props.texPath.NullOrEmpty())
                this.props.texPath = "FaceParts/Details/detail_empty";

            return GraphicDatabase.Get<Graphic_Multi>(this.props.texPath, ShaderDatabase.CutoutSkinOverlay, props.drawSize, ColorFor(pawn));
        }
    }
}
