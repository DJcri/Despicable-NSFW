using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EchoColonyIntegration
{
    /// <summary>
    /// Helper class to calculate and format interaction success chances for AI prompts.
    /// </summary>
    public static class InteractionSuccessHelper
    {
        // --- Private Balancing Constants ---
        private const float BaseSuccessChance = 0.60f;
        private const float HostileMultiplier = 0.25f;
        private const float RelationOpinionFactor = 0.003f;
        private const float AbilityImpactFactor = 0.15f;

        /// <summary>
        /// Calculates the probability of success for a single interaction type.
        /// </summary>
        private static float CalculateSuccessChance(Pawn heroPawn, Pawn pawnInteractingWith, string interactionLabel)
        {
            float successChance = BaseSuccessChance;
            float compositeAbility;

            // 1. Adjust Base Chance and Composite Stat based on Interaction Type
            switch (interactionLabel)
            {
                case "RecruitmentAttempt":
                case "ConvinceToEndRaidAttempt":
                case "MarriageProposal":
                    successChance *= 0.8f;
                    compositeAbility = heroPawn.GetStatValue(StatDefOf.NegotiationAbility);
                    break;

                case "Flirt":
                case "RomanceAttempt":
                    successChance *= 1.1f;
                    compositeAbility = heroPawn.GetStatValue(StatDefOf.SocialImpact);
                    break;

                case "Insult":
                case "Slight":
                    successChance *= 1.0f; // Neutral base, adjusted heavily by hostility later
                    compositeAbility = heroPawn.GetStatValue(StatDefOf.SocialImpact);
                    break;

                case "Chitchat":
                case "DeepTalk":
                case "KindWords":
                case "Breakup":
                default:
                    successChance *= 1.0f;
                    compositeAbility = heroPawn.GetStatValue(StatDefOf.SocialImpact);
                    break;
            }

            // 2. Add Factor: Relationship/Opinion
            int opinion = pawnInteractingWith.relations.OpinionOf(heroPawn);
            successChance += opinion * RelationOpinionFactor;

            // 3. Apply Factor: Composite Social Stats
            successChance += (compositeAbility - 1.0f) * AbilityImpactFactor;

            // 4. Apply Multiplier: Hostility/Alignment
            if (pawnInteractingWith.HostileTo(Faction.OfPlayer))
            {
                if (interactionLabel == "Insult" || interactionLabel == "Slight")
                {
                    // Easier to land an insult on a hostile pawn
                    successChance /= HostileMultiplier;
                }
                else
                {
                    // Harder to succeed in positive/persuasive action
                    successChance *= HostileMultiplier;
                }
            }
            else if (pawnInteractingWith.IsPrisonerOfColony)
            {
                successChance -= 0.15f; // General penalty for captive mood
            }

            // 5. Clamp the Result (5% to 95%)
            return Math.Max(0.05f, Math.Min(0.95f, successChance));
        }

        /// <summary>
        /// Generates a human-readable instruction based on the calculated success chance.
        /// </summary>
        private static string GetInstructionString(string interactionLabel, float finalChance)
        {
            string instruction;

            if (finalChance >= 0.80f)
            {
                instruction = $"VERY HIGH ({finalChance:P0}): Receptive, compliant, or yielding.";
            }
            else if (finalChance >= 0.60f)
            {
                instruction = $"HIGH ({finalChance:P0}): Receptive but cautious or considering.";
            }
            else if (finalChance >= 0.40f)
            {
                instruction = $"EVEN (40-60%): Genuine uncertainty, internal conflict, or stubborn refusal.";
            }
            else if (finalChance >= 0.20f)
            {
                instruction = $"LOW ({finalChance:P0}): Highly resistant, suspicious, or annoyed.";
            }
            else
            {
                instruction = $"VERY LOW ({finalChance:P0}): Aggressively defiant, mocking, or completely uninterested. Do not indicate success.";
            }

            if (interactionLabel == "Insult" || interactionLabel == "Slight")
            {
                instruction = instruction.Replace("Receptive, compliant", "Successful provocation")
                                         .Replace("yielding", "ready to fight");
            }

            return instruction;
        }

        /// <summary>
        /// Calculates the success chance for ALL interaction labels and formats them for the AI prompt.
        /// </summary>
        public static string GetAllSuccessInstructions(Pawn heroPawn, Pawn pawnInteractingWith, List<string> interactionLabels)
        {
            if (heroPawn == null || pawnInteractingWith == null || !interactionLabels.Any())
            {
                return "No balancing data available. Assume neutral response.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Below is the **Calculated Success Probability** for ALL player actions. Use this data to determine the tone and the 'success' boolean in your JSON response:");

            foreach (string label in interactionLabels)
            {
                float chance = CalculateSuccessChance(heroPawn, pawnInteractingWith, label);
                string instruction = GetInstructionString(label, chance);

                sb.AppendLine($"- **{label}:** {instruction}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
