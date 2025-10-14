using Despicable;
using EchoColony;
using HarmonyLib;
using RimWorld;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EchoColonyIntegration.Patches
{
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
                if (relationshipData == null || relationshipData["label"].IsNull || relationshipData["relationshipChange"].IsNull || relationshipData["moodEffect"].IsNull || relationshipData["success"].IsNull)
                    // If JSON is invalid or incomplete, let the original method handle the response.
                    return true;

                CommonUtil.DebugLog("[Despicable] - Found relationship data, applying changes");
                string interactionLabel = relationshipData["label"].Value;
                string moodEffect = relationshipData["moodEffect"].Value;
                int relationshipChange = relationshipData["relationshipChange"].AsInt;
                bool success = relationshipData["success"].AsBool;
                CommonUtil.DebugLog($"[Despicable] - label: {interactionLabel}, mood: {moodEffect}, opinion: {relationshipChange}, success: {success}");

                // Adjusts custom interaction labels for this mod's custom interactions.
                if (interactionLabel == "RomanceAttempt" || interactionLabel == "MarriageProposal")
                {
                    interactionLabel = "Despicable_" + interactionLabel;
                }

                // Applies the social interaction effects.
                InteractionDef interactionDef = DefDatabase<InteractionDef>.GetNamedSilentFail(interactionLabel);
                if (interactionDef != null)
                {
                    interactionDef.ignoreTimeSinceLastInteraction = true;
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

                    // Ensures romance and marriage attempts succeed or fail based on the AI's relationship change value.
                    if (interactionDef.Worker is Despicable.InteractionWorker_RomanceAttempt romanceWorker)
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
                    if (interactionDef.Worker is Despicable.InteractionWorker_MarriageProposal marriageWorker)
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
                }
                else /// Use special interaction utility if applicable
                {
                    CommonUtil.DebugLog("[Despicable x EchoColony] - InteractionDef not found, checking for special interaction handling...");
                    InteractionUtil.HandleSpecialInteraction(heroPawn, pawnInteractingWith, interactionLabel, success);
                }

                // Applies a mood effect to the interacting pawn.
                ThoughtDef moodEffectDef = ThoughtDef.Named(moodEffect + "MoodEffect");
                pawnInteractingWith?.needs?.mood?.thoughts?.memories?.TryGainMemory(moodEffectDef, heroPawn);

                // Triggers the actual social interaction in the game.
                heroPawn.interactions.TryInteractWith(pawnInteractingWith, interactionDef);
            }
            catch (Exception ex)
            {
                Log.Error("[Despicable x EchoColony] - Error parsing Gemini reply JSON: " + ex.Message);
            }

            // Returns true to allow the original method to run its course.
            return true;
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
