using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Despicable
{
    /// <summary>
    /// HANDLES majority of CALCULATIONS involving KARMA DATA, as well as its' UI COMPONENTS
    /// </summary>
    public static class KarmaUtil
    {
        public static float Modifier = 0.001f; /// Helps balancing values easier to read, and affects how fast karma is gained

        // Karma alignment abilities and their textures
        // Ordered from top to bottom, good karma to bad karma
        public static List<KarmaicAbilityDef> karmaAbilities = DefDatabase<KarmaicAbilityDef>.AllDefsListForReading.OrderByDescending(x => x.karmaRequirement).ToList();

        public static bool IsAbilityActive(KarmaicAbilityDef ability, Pawn pawn)
        {
            KarmaicAbilityDef instance = GetActiveAbilitiesFor(pawn).Find(a => a.defName == ability.defName);
            if (instance == null)
            {
                return false;
            }
            return true;
        }

        public static List<KarmaicAbilityDef> GetActiveAbilitiesFor(Pawn pawn)
        {
            List<KarmaicAbilityDef> activeAbilities = new List<KarmaicAbilityDef>();
            CompHero heroComp = pawn.TryGetComp<CompHero>();

            if (heroComp != null)
            {
                foreach (KarmaicAbilityDef ability in karmaAbilities)
                {
                    if (ability.karmaRequirement >= 0 && heroComp.karma >= ability.karmaRequirement)
                    {
                        activeAbilities.Add(ability);
                    }
                    else if (ability.karmaRequirement < 0 && heroComp.karma <= ability.karmaRequirement)
                    {
                        activeAbilities.Add(ability);
                    }
                }
            }

            return activeAbilities;
        }

        // Karma values for each deed
        public static Dictionary<string, float> karmaPerDeedValues = new Dictionary<string, float>()
        {
            { "Rescues", 2f },
            { "Treatments", 0.5f },
            { "CriminalArrests", 1f },
            { "HealthyPrisonersReleased", 5f },
            { "Charities", 5f },
            { "PawnsSentAway", -3f },
            { "InnocentsHarmed", -5f },
            { "ChildrenHarmed", -5f },
            { "HumansButchered", -2f },
            { "FalseArrests", -2f },
            { "EnslavementAttempts", -0.5f },
            { "Affairs", -3f },
        };

        public static void UpdateKarma(CompHero heroComp)
        {
            Dictionary<string, int> deedCountValues = heroComp.deedCountValues;
            float result = heroComp.karma / KarmaUtil.Modifier;
            Pawn pawn = heroComp.pawn;

            if (deedCountValues.NullOrEmpty())
                return;
            if (!heroComp.isHero)
                return;
            else
                RemoveKarmaicAbilities(pawn);

            foreach (var key in deedCountValues.Keys)
            {
                float karmaValue;
                if (!karmaPerDeedValues.TryGetValue(key, out karmaValue))
                {
                    continue;
                }

                int deedCount = deedCountValues[key];
                result += deedCount * karmaValue;
            }

            // Max and min
            result *= KarmaUtil.Modifier;
            result = result > 1f ? 1f : result;
            result = result < -1f ? -1f : result;

            heroComp.karma = result;

            // Add commandable abilities
            List<AbilityDef> commandAbilities = DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => {
                if (x.comps.Count > 0)
                {
                    if (x.comps[0] is CompProperties_AbilityKarmaic)
                        return true;
                }
                return false;
            }).ToList();

            if (CommonUtil.GetSettings().heroModuleEnabled)
            {
                foreach (AbilityDef abilityDef in commandAbilities)
                {
                    if (IsAbilityActive((abilityDef.comps[0] as CompProperties_AbilityKarmaic).karmaicAbilityDef, pawn))
                    {
                        pawn.abilities.GainAbility(abilityDef);
                    }
                }
            }
            else
            {
                RemoveKarmaicAbilities(pawn);
            }
        }

        // Function to remove all karmaic abilities from a pawn
        public static void RemoveKarmaicAbilities(Pawn pawn)
        {
            foreach (var ability in pawn.abilities.AllAbilitiesForReading.ToList())
            {
                // SAFELY check if the ability's definition has ANY component of the correct type
                if (ability?.def?.comps != null && ability.def.comps.Any(comp => comp is CompProperties_AbilityKarmaic))
                {
                    pawn.abilities.RemoveAbility(ability.def);
                }
            }
        }

        // UI GRAPHICS
        public static void DrawKarmaMeter(Rect inRect, float karma = 0, int meterTicks = 8, float lineThickness = 5f, float capWidth = 33f)
        {
            meterTicks -= 2;
            float xMid = inRect.center.x;
            float yMid = inRect.center.y;
            float abilityIconSize = 50f;
            float abilityIconMargin = abilityIconSize + (capWidth / 2) + 16f;
            float tickWidthModifier = 0.5f;

            // Draw main line
            Widgets.DrawBoxSolid(new Rect(xMid - (lineThickness / 2), inRect.yMin, lineThickness, inRect.height), Color.white);

            // Draw caps
            Widgets.DrawBoxSolid(new Rect(xMid - (capWidth / 2), inRect.yMin - (lineThickness / 2), capWidth, lineThickness), Color.white);
            Widgets.DrawBoxSolid(new Rect(xMid - (capWidth / 2), inRect.yMax - (lineThickness / 2), capWidth, lineThickness), Color.white);

            // Draw middle tick
            float midTickWidthModifier = 0.75f;
            Widgets.DrawBoxSolid(new Rect(xMid - (capWidth * midTickWidthModifier / 2), yMid - (lineThickness / 2), capWidth * midTickWidthModifier, lineThickness), Color.white);

            // Karma marker
            Rect markerIcon = new Rect(inRect);
            markerIcon.width = 32f;
            markerIcon.height = 32f;
            markerIcon.x = inRect.center.x - 16f + capWidth + 10f;
            markerIcon.y = inRect.center.y - 16f - (karma * inRect.height / 2); /// Point to where the hero lies on the karma meter
            Widgets.DrawTextureFitted(markerIcon, TexIcons.Marker, 1f);

            // Draw ticks in between
            // AND ability icons
            KarmaicAbilityDef karmaAbilityToDisplay = karmaAbilities[0];
            UIUtil.DrawSlot(new Rect(inRect.center.x - abilityIconMargin, inRect.yMin - (abilityIconSize / 2) - (lineThickness / 2), abilityIconSize, abilityIconSize), ContentFinder<Texture2D>.Get(karmaAbilityToDisplay.texPath), !(karma >= 1f), karmaAbilityToDisplay.description);
            karmaAbilityToDisplay = karmaAbilities[karmaAbilities.Count - 1];
            UIUtil.DrawSlot(new Rect(inRect.center.x - abilityIconMargin, inRect.yMax - (abilityIconSize / 2) - (lineThickness / 2), abilityIconSize, abilityIconSize), ContentFinder<Texture2D>.Get(karmaAbilityToDisplay.texPath), !(karma <= -1f), karmaAbilityToDisplay.description);
            for (int i = 0; i < (meterTicks / 2); i++)
            {
                float yPos = yMid - (lineThickness / 2) - ((i + 1) * (inRect.height / (meterTicks + 2)));
                bool abilityActivated = markerIcon.center.y <= yPos + (lineThickness / 2);
                Widgets.DrawBoxSolid(new Rect(xMid - (capWidth * tickWidthModifier / 2), yPos, capWidth * tickWidthModifier, lineThickness), Color.white);
                karmaAbilityToDisplay = karmaAbilities[((karmaAbilities.Count / 2) - 1) - i];
                UIUtil.DrawSlot(new Rect(inRect.center.x - abilityIconMargin, yPos - (abilityIconSize / 2), abilityIconSize, abilityIconSize), ContentFinder<Texture2D>.Get(karmaAbilityToDisplay.texPath), !abilityActivated, karmaAbilityToDisplay.description);

                yPos = yMid - (lineThickness / 2) + ((i + 1) * (inRect.height / (meterTicks + 2)));
                abilityActivated = markerIcon.center.y >= yPos + (lineThickness / 2);
                Widgets.DrawBoxSolid(new Rect(xMid - (capWidth * tickWidthModifier / 2), yPos, capWidth * tickWidthModifier, lineThickness), Color.white);
                karmaAbilityToDisplay = karmaAbilities[karmaAbilities.Count / 2 + i];
                UIUtil.DrawSlot(new Rect(inRect.center.x - abilityIconMargin, yPos - (abilityIconSize / 2), abilityIconSize, abilityIconSize), ContentFinder<Texture2D>.Get(karmaAbilityToDisplay.texPath), !abilityActivated, karmaAbilityToDisplay.description);
            }
        }

        public static void DrawDeeds(Rect inRect, Dictionary<string, int> deeds)
        {
            Rect labelRect = new Rect(inRect);
            labelRect.yMin += Text.LineHeight;
            Text.Font = GameFont.Small;

            foreach (string key in deeds.Keys)
            {
                labelRect.yMin += Text.LineHeight;
                int deedCount = deeds[key];
                string hexColor = karmaPerDeedValues[key] >= 0 ? HexColors.pastel_green : HexColors.pastel_red;
                string labelText = UIUtil.ColorTextHex(key.Translate(), hexColor);
                Widgets.Label(labelRect, labelText + ": " + deedCount.ToString());
            }
        }

        public static void DrawStatOffsets(Rect inRect, Pawn pawn)
        {
            Rect labelRect = new Rect(inRect);
            labelRect.yMin += Text.LineHeight;
            Text.Font = GameFont.Small;

            // Get stat defs with a karma offset
            List<StatDef> statDefs = DefDatabase<StatDef>.AllDefsListForReading.Where(statDef => statDef.GetStatPart<StatPart_KarmaOffset>() != null).ToList();

            if (statDefs.Count > 0)
            {
                foreach (StatDef statDef in statDefs)
                {
                    StatPart_KarmaOffset offsetPart = statDef.GetStatPart<StatPart_KarmaOffset>();
                    if (offsetPart != null)
                    {
                        float offset = offsetPart.KarmaOffset(pawn);

                        labelRect.yMin += Text.LineHeight;
                        string labelText = statDef.label.CapitalizeFirst();

                        // Assign color and add or subtract symbol to offset label before rendering
                        string hexColor = offset >= 0f ? HexColors.pastel_green : HexColors.pastel_red;
                        string plusMinus = offset < 0 ? "" : "+";
                        string valueLabel = (offset > 0f || offset < 0f) ? UIUtil.ColorTextHex(plusMinus + offset.ToString("0.00") + "%", hexColor) : plusMinus + offset.ToString("0.00") + "%"; /// Round value to 2 decimal points in string
                        Widgets.Label(labelRect, labelText + ": " + valueLabel);
                    }
                }
            }
        }
    }
}