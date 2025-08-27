using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Noise;

namespace Despicable
{
    /// <summary>
    /// Offset modifier to a parent stat
    /// </summary>
    public class StatPart_KarmaOffset : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Pawn pawn && ActiveFor(pawn))
            {
                return "StatsReport_KarmaOffset".Translate() + ": " + KarmaOffset(pawn).ToString("0.00") + "%";
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            val += KarmaOffset(req.Pawn);
        }

        public float KarmaOffset(Pawn pawn)
        {
            CompHero heroComp = pawn.TryGetComp<CompHero>();
            float offset = 0f;
            float debuffMultiplier = 0.75f;

            if (heroComp != null)
            {
                StatDef stat = parentStat;

                if (stat != null)
                {
                    float karma = heroComp.karma;
                    switch (stat.defName)
                    {
                        // Set the maximum buff offset the player hero can receive
                        case "TradePriceImprovement":
                            offset = 15f;
                            break;
                        case "ConversionPower":
                            offset = 30f;
                            break;
                        case "SocialImpact":
                            offset = 30f;
                            break;
                        case "TameAnimalChance":
                            offset = 20f;
                            break;
                        case "BondAnimalChanceFactor":
                            offset = 15f;
                            break;
                        /// Bad karma required for these offsets, hence negative floats
                        case "ArrestSuccessChance":
                            offset = -15f;
                            break;
                        case "TrainAnimalChance":
                            offset = -25f;
                            break;
                        case "NegotiationAbility":
                            offset = -10f;
                            break;
                        case "SuppressionPower":
                            offset = -20f;
                            break;
                        case "CertaintyLossFactor":
                            offset = 25f;
                            break;
                    }

                    // Applies karma value to the offset
                    offset *= karma;

                    /// Buffs will become debuffs after switching sides
                    /// of the karma spectrum. This can also
                    /// provide an incentive to stay neutral
                    /// on the karma scale.

                    // Lowers the value if the offset became a debuff
                    if (offset < 0f) offset *= debuffMultiplier;

                    // For certainty loss a positive value "IS" a debuff, so do the inverse
                    if (stat.defName == "CertaintyLossFactor")
                    {
                        offset /= debuffMultiplier;
                        if (offset > 0f) offset *= debuffMultiplier;
                    }

                }
            }

            return offset;
        }

        private bool ActiveFor(Pawn pawn)
        {
            return true;
        }
    }
}
