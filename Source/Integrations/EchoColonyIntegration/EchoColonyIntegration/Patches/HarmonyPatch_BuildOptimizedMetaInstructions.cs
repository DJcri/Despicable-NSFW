using Despicable;
using EchoColony;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EchoColonyIntegration.Patches
{
    // Harmony patch that modifies the AI's prompt to include information about the hero pawn.
    [HarmonyPatch(typeof(ColonistPromptContextBuilder), "BuildOptimizedMetaInstructions")]
    public static class HarmonyPatch_AddHeroPromptContext
    {
        /// <summary>
        /// Custom mood effects the AI can use in its JSON response.
        /// </summary>
        private static readonly List<String> moodEffects = new List<string> {
            "Tense",
            "Neutral",
            "Hopeful",
            "Jubilant"
        };
        /// <summary>
        /// Custom interaction labels the AI can use in its JSON response.
        /// These labels should correspond to the types of interactions the player can have with other pawns.
        /// If no InteractionDef matches, SpecialInteractionUtility should have a function to
        /// handle it if it is a valid label.
        /// SI = Special Interaction
        /// </summary>
        private static readonly List<String> interactionLabels = new List<string> { 
            "Chitchat",
            "DeepTalk",
            "Insult",
            "Slight",
            "KindWords",
            "Flirt", // SI
            "RomanceAttempt",
            "Breakup",
            "MarriageProposal",
            "RecruitmentAttempt", // SI
            "ConvinceToEndRaidAttempt" // SI
        };

        // Postfix method that adds instructions for the AI to recognize the player's hero pawn.
        public static void Postfix(ref string __result, ref Pawn pawn)
        {
            Pawn heroPawn = HeroUtil.FindHero();
            if (heroPawn == null)
                return;

            string heroPawnName = heroPawn?.Name.ToStringFull ?? "Player";
            if (!heroPawnName.NullOrEmpty())
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(__result))
                {
                    stringBuilder.AppendLine(__result);
                }
                else
                {
                    stringBuilder.Append(__result);
                }
                stringBuilder.AppendLine("The player's self-insert pawn is " + heroPawnName + ". Respond as if you are speaking to the self-insert pawn.");
                stringBuilder.AppendLine("**DON'T FORGET** Your response MUST consist of two parts, a dialogue and a JSON object.");
                stringBuilder.AppendLine("The dialogue MUST come first as a simple, un-keyed string.");
                stringBuilder.AppendLine("The JSON object MUST be appended at the end of the dialogue on its own line.");
                stringBuilder.AppendLine("The JSON object MUST be **valid JSON format**. All keys and string values must be enclosed in double quotes.");
                stringBuilder.AppendLine("The JSON object MUST contain the following keys:");
                stringBuilder.AppendLine("- \"relationshipChange\": An integer from -100 to 100.");
                stringBuilder.AppendLine("- \"moodEffect\": A string from a predefined list.");
                stringBuilder.AppendLine("- \"label\": A string from a predefined list.");
                stringBuilder.AppendLine("- \"success\": A bool that describes whether the player was successful in negotiating, flirting, convincing etc.");
                stringBuilder.AppendLine($"The predefined list for moodEffect is: \"{string.Join("\", \"", moodEffects)}\".");
                stringBuilder.AppendLine($"The predefined list for label is: \"{string.Join("\", \"", interactionLabels)}\".");
                stringBuilder.AppendLine("Example response:");
                stringBuilder.AppendLine("Hi, the weather is nice today. *She announces gladly*");
                stringBuilder.AppendLine("{\"label\": \"Chat\", \"relationshipChange\": 5, \"moodEffect\": \"Neutral\"}");

                // Append instructions for when a self-insert pawn is found
                // and the roleplay responses must be directed at them.
                if (!heroPawnName.NullOrEmpty())
                {
                    if (!string.IsNullOrEmpty(__result))
                    {
                        stringBuilder.AppendLine(__result);
                    }
                    else
                    {
                        stringBuilder.Append(__result);
                    }
                    stringBuilder.AppendLine("The player's self-insert pawn is " + heroPawnName + ". Respond as if you are speaking to the self-insert pawn.");
                    stringBuilder.AppendLine("**DON'T FORGET** Your response MUST consist of two parts, a dialogue and a JSON object.");
                    stringBuilder.AppendLine("The dialogue MUST come first as a simple, un-keyed string.");
                    stringBuilder.AppendLine("The JSON object MUST be appended at the end of the dialogue on its own line.");
                    stringBuilder.AppendLine("The JSON object MUST be **valid JSON format**. All keys and string values must be enclosed in double quotes.");
                    stringBuilder.AppendLine("The JSON object MUST contain the following keys:");
                    stringBuilder.AppendLine("- \"relationshipChange\": An integer from -100 to 100.");
                    stringBuilder.AppendLine("- \"moodEffect\": A string from a predefined list.");
                    stringBuilder.AppendLine("- \"label\": A string from a predefined list.");
                    stringBuilder.AppendLine("- \"success\": A bool that describes whether the player was successful in negotiating, flirting, convincing etc.");
                    stringBuilder.AppendLine($"The predefined list for moodEffect is: \"{string.Join("\", \"", moodEffects)}\".");
                    stringBuilder.AppendLine($"The predefined list for label is: \"{string.Join("\", \"", interactionLabels)}\".");
                    stringBuilder.AppendLine("Example response:");
                    stringBuilder.AppendLine("Hi, the weather is nice today. *She announces gladly*");
                    stringBuilder.AppendLine("{\"label\": \"Chat\", \"relationshipChange\": 5, \"moodEffect\": \"Neutral\"}");
                    __result = stringBuilder.ToString();
                }

                __result = stringBuilder.ToString();
            }
        }
    }
}
