using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace Despicable
{
    public class FaceGizmo : Command
    {
        private Pawn pawn;
        public override string Desc => "Customize face";
        public override string Label => "FaceGizmo".Translate();

        public FaceGizmo(Pawn pawn)
        {
            icon = TexIcons.FaceGizmo;
            this.pawn = pawn;
        }

        public override void ProcessInput(Event ev)
        {
            // Open hero screen
            if (ev.button == ((int)MouseButton.LeftMouse))
            {
                CompFaceParts facePartsComp = pawn.TryGetComp<CompFaceParts>();
                /*
                if (CommonUtil.GetSettings().debugMode)
                    Find.WindowStack.Add(new Dialog_AnimBuilder());
                else
                */
                Find.WindowStack.Add(new Dialog_FaceCustomizer(pawn));
            }
        }
    }
}
