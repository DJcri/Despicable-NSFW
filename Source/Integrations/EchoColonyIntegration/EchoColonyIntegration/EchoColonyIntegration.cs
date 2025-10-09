using EchoColony;
using HarmonyLib;
using RimWorld;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    // Mod class for the Despicable mod's integration with EchoColony.
    // Initializes Harmony to patch the EchoColony mod's methods.
    public class EchoColonyIntegration : Mod
    {
        public const float OPINION_CHANGE_MULTIPLIER = 0.2f;

        public EchoColonyIntegration(ModContentPack content)
            : base(content)
        {
            // Initializes and applies all Harmony patches defined in this mod.
            Harmony harmony = new Harmony("Despicable.EchoColonyIntegration");
            harmony.PatchAll();
        }
    }

    // Harmony patch that modifies the AI's prompt to include information about the hero pawn.
    [HarmonyPatch(typeof(ColonistPromptContextBuilder), "BuildOptimizedMetaInstructions")]
    public static class HarmonyPatch_AddHeroPromptContext
    {
        // Postfix method that adds instructions for the AI to recognize the player's hero pawn.
        public static void Postfix(ref string __result, ref Pawn pawn)
        {
            Pawn heroPawn = HeroUtil.FindHero();
            if (heroPawn == null)
                return;

            string heroPawnName = heroPawn?.Name.ToStringFull ?? "";
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
                stringBuilder.AppendLine("Your response must consist of two parts, a dialogue and a JSON object.");
                stringBuilder.AppendLine("The dialogue must come first as a simple, un-keyed string.");
                stringBuilder.AppendLine("The JSON object must be appended at the end of the dialogue on its own line.");
                stringBuilder.AppendLine("The JSON object must be **valid JSON format**. All keys and string values must be enclosed in double quotes.");
                stringBuilder.AppendLine("The JSON object must contain the following keys:");
                stringBuilder.AppendLine("- \"relationshipChange\": An integer from -100 to 100.");
                stringBuilder.AppendLine("- \"moodEffect\": A string from a predefined list.");
                stringBuilder.AppendLine("- \"label\": A string from a predefined list.");
                stringBuilder.AppendLine("The predefined list for moodEffect is: \"Tense\", \"Neutral\", \"Hopeful\", \"Jubilant\".");
                stringBuilder.AppendLine("The predefined list for label is: \"Chitchat\", \"DeepTalk\", \"Insult\", \"Slight\", \"KindWords\", \"RomanceAttempt\", \"Breakup\", \"MarriageProposal\".");
                stringBuilder.AppendLine("Example response:");
                stringBuilder.AppendLine("Hi, the weather is nice today.");
                stringBuilder.AppendLine("{\"label\": \"Chat\", \"relationshipChange\": 5, \"moodEffect\": \"Neutral\"}");
                __result = stringBuilder.ToString();
            }
        }
    }

    // Harmony patch to store the current interacting pawn.
    [HarmonyPatch(typeof(ColonistChatWindow), MethodType.Constructor, new Type[] { typeof(Pawn) })]
    public static class HarmonyPatch_ColonistChatWindow
    {
        // Postfix method that stores which pawn the hero is currently talking to in a custom CompHero component.
        public static void Postfix(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return;
                }
                Pawn heroPawn = HeroUtil.FindHero();
                if (heroPawn != null)
                {
                    CompHero comp = ((Thing)heroPawn).TryGetComp<CompHero>();
                    if (comp != null)
                    {
                        comp.pawnInteractingWith = pawn;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Despicable] - Error patching chat window constructor: " + ex.Message);
            }
        }
    }

    // Harmony patch that modifies the AI's response to apply social effects.
    [HarmonyPatch(typeof(GeminiAPI), "ParseGeminiReply")]
    public static class HarmonyPatch_ParseGeminiReply
    {
        // Postfix method to clean the dialogue by removing the JSON part before displaying it.
        public static void Postfix(ref string __result)
        {
            CommonUtil.DebugLog("[Despicable] - Attempting to clean dialogue for user...");
            __result = CleanDialogue(__result);
        }

        // Prefix method that parses a JSON object from the AI's response and applies the effects.
        public static bool Prefix(string json, ref string __result)
        {
            if (json.NullOrEmpty())
                return true;

            Pawn heroPawn = HeroUtil.FindHero();
            CommonUtil.DebugLog("[Despicable] - Checking for hero pawn...");
            if (heroPawn == null)
                return true;

            Pawn pawnInteractingWith = ((Thing)heroPawn).TryGetComp<CompHero>()?.pawnInteractingWith;
            CommonUtil.DebugLog("[Despicable] - Checking for pawn being interacted with...");
            if (pawnInteractingWith == null)
                return true;

            CommonUtil.DebugLog("[Despicable] - Looking for relationship data...");
            try
            {
                // Extracts the JSON object from the response.
                JSONNode relationshipData = JsonHelper.ExtractSubJsonFromResponse(json);
                if (relationshipData == null || relationshipData["label"] == null || relationshipData["relationshipChange"] == null || relationshipData["moodEffect"] == null)
                {
                    // If JSON is invalid or incomplete, let the original method handle the response.
                    return true;
                }
                CommonUtil.DebugLog("[Despicable] - Found relationship data, applying changes");
                string interactionLabel = relationshipData["label"].Value;
                string moodEffect = relationshipData["moodEffect"].Value;
                int relationshipChange = relationshipData["relationshipChange"].AsInt;
                CommonUtil.DebugLog($"[Despicable] - label: {interactionLabel}, mood: {moodEffect}, opinion: {relationshipChange}");

                // Adjusts custom interaction labels for this mod's custom interactions.
                if (interactionLabel == "RomanceAttempt" || interactionLabel == "MarriageProposal")
                {
                    interactionLabel = "Despicable_" + interactionLabel;
                }

                // Applies the social interaction effects.
                InteractionDef interactionDef = DefDatabase<InteractionDef>.GetNamed(interactionLabel);
                interactionDef.ignoreTimeSinceLastInteraction = true;
                if (interactionDef != null)
                {
                    ThoughtDef recipientThought = interactionDef.recipientThought;
                    if (recipientThought != null)
                    {
                        ThoughtStage thoughtStage = recipientThought.stages[0];
                        if (thoughtStage != null)
                        {
                            _ = thoughtStage.baseOpinionOffset;
                            if (true)
                            {
                                // Overrides the default opinion change with the value from the AI's JSON.
                                interactionDef.recipientThought.stages[0].baseOpinionOffset = (float)relationshipChange * 0.2f;
                            }
                        }
                    }
                }

                // Ensures romance and marriage attempts succeed or fail based on the AI's relationship change value.
                if (interactionDef.Worker is InteractionWorker_RomanceAttempt romanceWorker)
                {
                    if (relationshipChange > 0)
                    {
                        romanceWorker.BaseSuccessChance = 1f;
                    }
                    else
                    {
                        romanceWorker.BaseSuccessChance = -1f;
                    }
                }
                if (interactionDef.Worker is InteractionWorker_MarriageProposal marriageWorker)
                {
                    if (relationshipChange > 0)
                    {
                        marriageWorker.BaseAcceptanceChance = 1f;
                    }
                    else
                    {
                        marriageWorker.BaseAcceptanceChance = -1f;
                    }
                }

                // Applies a mood effect to the interacting pawn.
                ThoughtDef moodEffectDef = ThoughtDef.Named(moodEffect + "MoodEffect");
                pawnInteractingWith?.needs?.mood?.thoughts?.memories?.TryGainMemory(moodEffectDef, heroPawn);

                // Triggers the actual social interaction in the game.
                heroPawn.interactions.TryInteractWith(pawnInteractingWith, interactionDef);
            }
            catch (Exception ex)
            {
                Log.Error("[Despicable] - Error parsing Gemini reply JSON: " + ex.Message);
            }

            // Returns true to allow the original method to run its course.
            return true;
        }

        // Patch for "TryInteractWith" to ignore checking for certain conditions if called from this mod.
        [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
        public static class TryInteractWith_NamespaceCheck_Patch
        {
            public static bool Prefix(Pawn recipient, InteractionDef intDef, ref bool __result)
            {
                StackTrace stackTrace = new StackTrace();

                for (int i = 1; i < stackTrace.FrameCount; i++)
                {
                    StackFrame frame = stackTrace.GetFrame(i);
                    MethodBase callingMethod = frame.GetMethod();

                    Type callingType = callingMethod?.DeclaringType;

                    if (callingType != null)
                    {
                        if (callingType.FullName.StartsWith("Despicable", StringComparison.Ordinal))
                        {
                            CommonUtil.DebugLog("[Despicable] - Detected call to TryInteractWith from Despicable mod, bypassing checks.");
                            __result = true;
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        // Utility method to extract just the dialogue from the AI's full response.
        public static string CleanDialogue(string fullResponse)
        {
            int jsonStartIndex = fullResponse.IndexOf("{");
            if (jsonStartIndex != -1)
            {
                return fullResponse.Substring(0, jsonStartIndex).Trim();
            }
            return fullResponse.Trim();
        }
    }
}