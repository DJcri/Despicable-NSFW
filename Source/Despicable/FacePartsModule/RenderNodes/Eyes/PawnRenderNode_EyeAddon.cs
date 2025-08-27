using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Despicable;
using RimWorld;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class PawnRenderNode_EyeAddon : PawnRenderNode
    {
        public PawnRenderNode_EyeAddon(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
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

            return GraphicDatabase.Get<Graphic_Multi>(this.TexPathFor(pawn), ShaderDatabase.CutoutSkinOverlay, props.drawSize, ColorFor(pawn));
        }
    }
}
