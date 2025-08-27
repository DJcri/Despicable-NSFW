using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class Dialog_FaceCustomizer : Window
    {
        public override Vector2 InitialSize => new Vector2(750f, 500f);
        private static readonly Vector2 ButSize = new Vector2(200f, 40f);
        CompFaceParts facePartsComp;

        private Pawn pawn;
        private int eyeStyleIndex;
        private int mouthStyleIndex;
        public List<FacePartStyleDef> facePartStyles = DefDatabase<FacePartStyleDef>.AllDefsListForReading;
        public List<FacePartStyleDef> eyeStyles;
        public List<FacePartStyleDef> mouthStyles;
        private bool shouldUpdate = false;

        public Dialog_FaceCustomizer(Pawn pawn)
        {
            if (mouthStyles.NullOrEmpty() || eyeStyles.NullOrEmpty())
            {
                eyeStyles = DefDatabase<FacePartStyleDef>.AllDefsListForReading.Where(s => s.renderNodeTag.defName == "FacePart_Eye").ToList();
                mouthStyles = DefDatabase<FacePartStyleDef>.AllDefsListForReading.Where(s =>
                {
                    if (s.renderNodeTag.defName != "FacePart_Mouth")
                        return false;
                    if (s.requiredGender != null && s.requiredGender != (byte)pawn.gender)
                        return false;

                    return true;
                }).ToList();
            }

            this.pawn = pawn;
            facePartsComp = pawn.TryGetComp<CompFaceParts>();
            if (facePartsComp == null) return;

            // Get indexes
            // For eyes
            for (int i = 0; i < eyeStyles.Count; i++)
            {
                if (eyeStyles[i] == facePartsComp.eyeStyleDef)
                {
                    eyeStyleIndex = i;
                }
            }
            // For mouth
            for (int i = 0; i < mouthStyles.Count; i++)
            {
                if (mouthStyles[i] == facePartsComp.mouthStyleDef)
                {
                    mouthStyleIndex = i;
                }
            }

            pawn.TryGetComp(out facePartsComp);
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (pawn == null) return;

            // Create header
            Text.Font = GameFont.Medium;
            Rect titleLabel = new Rect(inRect);
            titleLabel.height = Text.LineHeight * 2f;
            Widgets.Label(titleLabel, "Face Customizer");

            // Draw pawn
            Rect pawnWindow = new Rect(inRect);
            pawnWindow.width *= 0.5f;
            pawnWindow.yMin = titleLabel.yMax;
            pawnWindow.height = inRect.height * 0.8f;
            UIUtil.DrawPawnImage(pawn, pawnWindow, shouldUpdate, renderHeadgear: false);
            if (shouldUpdate)
                shouldUpdate = false;

            // Draw eye selector
            Text.Font = GameFont.Small;
            Rect labelRect = new Rect(inRect);
            labelRect.xMin = inRect.xMax - ButSize.x - ButSize.y - 8f;
            labelRect.y = titleLabel.yMax + 100f;
            labelRect.height = ButSize.y;
            labelRect.width = 150f;
            Widgets.ButtonText(labelRect, eyeStyles[eyeStyleIndex].label);

            Rect backButtonRect = new Rect(labelRect);
            backButtonRect.width = ButSize.y;
            backButtonRect.x = labelRect.x - ButSize.y - 4f;

            Rect nextButtonRect = new Rect(backButtonRect);
            nextButtonRect.x = labelRect.xMax + 4f;

            if (Widgets.ButtonText(backButtonRect, "<"))
            {
                if (eyeStyleIndex > 0)
                    eyeStyleIndex--;
                else
                    eyeStyleIndex = eyeStyles.Count - 1;

                facePartsComp.eyeStyleDef = eyeStyles[eyeStyleIndex];
                shouldUpdate = true;
            }
            if (Widgets.ButtonText(nextButtonRect, ">"))
            {
                if (eyeStyleIndex < (eyeStyles.Count - 1))
                    eyeStyleIndex++;
                else
                    eyeStyleIndex = 0;

                facePartsComp.eyeStyleDef = eyeStyles[eyeStyleIndex];
                shouldUpdate = true;
            }

            // Draw mouth selector
            labelRect.y += ButSize.y + 16f;
            Widgets.ButtonText(labelRect, mouthStyles[mouthStyleIndex].label);

            backButtonRect = new Rect(labelRect);
            backButtonRect.width = ButSize.y;
            backButtonRect.x = labelRect.x - ButSize.y - 4f;

            nextButtonRect = new Rect(backButtonRect);
            nextButtonRect.x = labelRect.xMax + 4f;

            if (Widgets.ButtonText(backButtonRect, "<"))
            {
                if (mouthStyleIndex > 0)
                    mouthStyleIndex--;
                else
                    mouthStyleIndex = mouthStyles.Count - 1;

                facePartsComp.mouthStyleDef = mouthStyles[mouthStyleIndex];
                shouldUpdate = true;
            }
            if (Widgets.ButtonText(nextButtonRect, ">"))
            {
                if (mouthStyleIndex < (mouthStyles.Count - 1))
                    mouthStyleIndex++;
                else
                    mouthStyleIndex = 0;

                facePartsComp.mouthStyleDef = mouthStyles[mouthStyleIndex];
                shouldUpdate = true;
            }

            // Draw buttons
            DrawCloseButton(inRect);
        }

        private void DrawCloseButton(Rect inRect)
        {
            // Center button's x to parent
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(inRect.center.x - (ButSize.x / 2), inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Close".Translate()))
            {
                Close();
            }
        }
    }
}
