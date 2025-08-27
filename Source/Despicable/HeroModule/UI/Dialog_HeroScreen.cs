using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Despicable
{
    // Hero screen
    [StaticConstructorOnStartup]
    public class Dialog_HeroScreen : Window
    {
        public override Vector2 InitialSize => new Vector2(750f, 750f);
        private static readonly Vector2 ButSize = new Vector2(200f, 40f);
        CompHero heroComp;

        private Pawn pawn;

        public Dialog_HeroScreen(Pawn pawn)
        {
            this.pawn = pawn;
            pawn.TryGetComp(out heroComp);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // If the pawn was chosen to be the hero, display stats
            if (heroComp.isHero)
            {
                float columnWidth = inRect.width * 0.3f;

                // Create header
                Text.Font = GameFont.Medium;
                Rect titleLabel = new Rect(inRect);
                titleLabel.height = Text.LineHeight * 2f;
                Widgets.Label(titleLabel, "HeroMenuTitle".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));

                // Resize window to fit content and add padding
                inRect.yMin = titleLabel.yMax + 4f;

                // Render sections
                // Column 1 - Title, Karma bar
                float iconSize = 64f;

                Rect column1 = inRect;
                column1.yMin += iconSize + 16f;
                column1.yMax -= ButSize.y + 20f + iconSize;
                column1.width = columnWidth;

                // Draw meter
                KarmaUtil.DrawKarmaMeter(column1, heroComp.karma);

                // Draw icons
                Rect goodKarmaIcon = new Rect(inRect);
                goodKarmaIcon.yMin = column1.yMin - iconSize - 8f;
                goodKarmaIcon.width = iconSize;
                goodKarmaIcon.height = iconSize;
                goodKarmaIcon.x = column1.center.x - (iconSize / 2);
                Widgets.DrawTextureFitted(goodKarmaIcon, TexIcons.GoodKarma, 1f);

                Rect badKarmaIcon = new Rect(inRect);
                badKarmaIcon.yMin = column1.yMax + 8f;
                badKarmaIcon.width = iconSize;
                badKarmaIcon.height = iconSize;
                badKarmaIcon.x = column1.center.x - (iconSize / 2);
                Widgets.DrawTextureFitted(badKarmaIcon, TexIcons.BadKarma, 1f);

                // Set font for subheaders
                Text.Font = GameFont.Small;

                // Column 2 - Deeds
                Rect column2 = inRect;
                column2.xMin = column1.xMax + 10f;
                column2.yMin = titleLabel.yMax;
                column2.yMax -= ButSize.y + 4f;
                column2.width = columnWidth;
                Rect deedsSectionLabel = new Rect(column2);
                deedsSectionLabel.height = Text.LineHeight + 4f;
                Widgets.Label(deedsSectionLabel, "DeedsSectionLabel".Translate() + ": ");
                Widgets.DrawLineHorizontal(deedsSectionLabel.xMin, deedsSectionLabel.yMax, deedsSectionLabel.width * 0.5f);
                KarmaUtil.DrawDeeds(column2, heroComp.GetDeeds());

                // Column3 - Stats
                Rect column3 = inRect;
                column3.xMin = column2.xMax + 10f;
                column3.yMin = titleLabel.yMax;
                column3.yMax -= ButSize.y + 4f;
                Rect statOffsetsSectionLabel = new Rect(column3);
                statOffsetsSectionLabel.height = Text.LineHeight + 4f;
                Widgets.Label(statOffsetsSectionLabel, "StatOffsetsSectionLabel".Translate() + ": ");
                Widgets.DrawLineHorizontal(statOffsetsSectionLabel.xMin, statOffsetsSectionLabel.yMax, statOffsetsSectionLabel.width * 0.5f);
                KarmaUtil.DrawStatOffsets(column3, pawn);
            }
            else
            // Render option to select this pawn as the hero
            {
                // Create header
                Text.Font = GameFont.Medium;
                Rect titleLabel = new Rect(inRect);
                titleLabel.height = Text.LineHeight * 2f;
                Widgets.Label(titleLabel, "ChooseHeroDialog".Translate(Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name)));

                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(new Rect(inRect.center.x - (ButSize.x / 2), inRect.yMax - (ButSize.y * 2 + 4f), ButSize.x, ButSize.y), "ChooseHeroButton".Translate()))
                {
                    heroComp.isHero = true;
                    WindowUpdate();
                }
            }

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