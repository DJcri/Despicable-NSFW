using Despicable;
using EchoColony;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace EchoColonyIntegration
{
    /// <summary>
    /// Revises the system prompt for AI chat to include detailed pawn information and balancing data.
    /// </summary>
    [HarmonyPatch(typeof(ColonistPromptContextBuilder), "BuildSystemPrompt")]
    public static class HarmonyPatch_ReviseSystemPrompt
    {
        /// <summary>
        /// Custom mood effects the AI can use in its JSON response.
        /// </summary>
        private static readonly List<String> moodEffects = new List<string> {
            "Tense",
            "Neutral", // Added neutral back for completeness
            "Hopeful",
            "Jubilant"
        };
        /// <summary>
        /// Custom interaction labels the AI can use in its JSON response.
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
            "ConvinceToEndRaidAttempt", // SI
            "SnapOutOfMentalBreakAttempt" // SI
        };

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref string __result)
        {
            // --- 1. Get Pawn Info ---
            string fullName = pawn.Label;
            string genderLabel = pawn.gender.ToString();
            string xenotypeLabel = pawn.genes?.Xenotype?.LabelCap ?? "Baseline Human";
            string factionName = pawn.Faction?.Name ?? "None (Wild Pawn)";
            string technologyLevel = pawn.Faction?.def?.techLevel.ToString() ?? "Unknown";
            string settlementName = Find.CurrentMap.info?.parent?.LabelCap ?? "Unknown Settlement";

            // --- 2. Determine Alignment (Strict Hierarchy) ---
            string alignmentLabel;

            // Colony Status (Most specific, takes highest priority)
            if (pawn.IsPrisonerOfColony)
            {
                alignmentLabel = "Prisoner";
            }
            else if (pawn.IsSlaveOfColony)
            {
                alignmentLabel = "Slave";
            }
            else if (pawn.Faction == Faction.OfPlayer)
            {
                alignmentLabel = "Colonist";
            }
            // External Factions
            else if (pawn.Faction != null)
            {
                if (pawn.Faction.HostileTo(Faction.OfPlayer))
                {
                    if (pawn.Faction == Faction.OfInsects)
                    {
                        alignmentLabel = "Enemy Insectoid";
                    }
                    else if (pawn.Faction == Faction.OfMechanoids)
                    {
                        alignmentLabel = "Enemy Mechanoid";
                    }
                    else if (pawn.Faction == Faction.OfPirates)
                    {
                        alignmentLabel = "Pirate Raider";
                    }
                    else
                    {
                        alignmentLabel = "Hostile Enemy"; // Generic hostile faction
                    }
                }
                else if (pawn.Faction.AllyOrNeutralTo(Faction.OfPlayer))
                {
                    if (pawn.Faction == Faction.OfTradersGuild)
                    {
                        alignmentLabel = "Trader's Guild Member";
                    }
                    else if (pawn.Faction == Faction.OfAncients)
                    {
                        alignmentLabel = "Ancient Visitor";
                    }
                    else
                    {
                        alignmentLabel = "Visitor"; // Allied or neutral faction
                    }
                }
                else
                {
                    alignmentLabel = "Visitor"; // Fallback for uncategorized external factions
                }
            }
            // Wild or Unfactioned Pawn (Default fallback)
            else
            {
                alignmentLabel = "Solo Traveler";
            }

            // --- 3. Construct the Cohesive System Prompt ---
            StringBuilder stringBuilder = new StringBuilder();

            // Set the character's base identity
            stringBuilder.AppendLine($"**Pawn Identity:** You are {fullName}, a {technologyLevel} {alignmentLabel} in RimWorld.");
            stringBuilder.AppendLine($"**Pawn Details:** Gender: {genderLabel} | Xenotype: {xenotypeLabel}.");
            stringBuilder.AppendLine($"**Location & Affiliation:** Faction: \"{factionName}\" | Location: {settlementName}.");

            // Append the original prompt result (the base game prompt), if it exists.
            if (!string.IsNullOrEmpty(__result))
            {
                stringBuilder.AppendLine(Environment.NewLine + "--- Original Prompt Context ---");
                stringBuilder.AppendLine(__result);
                stringBuilder.AppendLine("-----------------------------");
            }

            // Core Instruction
            stringBuilder.AppendLine(Environment.NewLine + "**CORE INSTRUCTION:** Speak only from your pawn's perspective and stay strictly in character.");
            stringBuilder.AppendLine("Even when the chance of success is LOW, if the player's argument is exceptionally convincing and character-appropriate, mitigate the negative tone and reduce the severity of the 'relationshipChange' in the JSON, showing internal conflict instead of simple dismissal.");

            // Append Hero Instructions and Response Format
            Pawn heroPawn = HeroUtil.FindHero();
            if (heroPawn != null && !heroPawn.Label.NullOrEmpty())
            {
                // VVVVVV INSERTION POINT FOR BALANCE LOGIC VVVVVV
                // Calculate success instructions for all possible labels and insert them here.
                string successInstruction = InteractionSuccessHelper.GetAllSuccessInstructions(heroPawn, pawn, interactionLabels);
                stringBuilder.AppendLine(Environment.NewLine + "**BALANCE INSTRUCTION: ACTION PROBABILITIES**");
                stringBuilder.AppendLine(successInstruction);
                // ^^^^^^ END INSERTION POINT ^^^^^^

                // Instruction to address the player's pawn directly
                stringBuilder.AppendLine($"The player's self-insert pawn is named {heroPawn.Label}. Respond as if you are directly speaking to them.");

                // Response Format Instructions
                stringBuilder.AppendLine(Environment.NewLine + "--- AI RESPONSE FORMAT ---");
                stringBuilder.AppendLine("**1. DIALOGUE:** The first part must be a simple, un-keyed string containing ONLY the character's response.");
                stringBuilder.AppendLine("**2. JSON OBJECT:** The second part MUST be a single, valid JSON object on its own line immediately following the dialogue.");
                stringBuilder.AppendLine("The JSON object MUST contain the following keys and follow these constraints:");
                stringBuilder.AppendLine("- \"label\": A string from the predefined list: " + string.Join(", ", interactionLabels.Select(s => $"\"{s}\"")) + ".");
                stringBuilder.AppendLine("- \"relationshipChange\": An integer value ranging from -100 to 100.");
                stringBuilder.AppendLine("- \"moodEffect\": A string from the predefined list: " + string.Join(", ", moodEffects.Select(s => $"\"{s}\"")) + ".");
                stringBuilder.AppendLine("- \"success\": A boolean (true or false) indicating the outcome of the player's interaction attempt (e.g., successful recruitment, insult, flirt).");
                stringBuilder.AppendLine(Environment.NewLine + "Example Response:");
                stringBuilder.AppendLine($"\"I've been thinking about your offer. It might be time for me to join your colony.\" *The raider sighs, looking defeated*");
                stringBuilder.AppendLine("{\"label\": \"RecruitmentAttempt\", \"relationshipChange\": 20, \"moodEffect\": \"Hopeful\", \"success\": true}");
            }

            // Finalize the result
            __result = stringBuilder.ToString();
        }
    }

    // =======================================================================
    // CLASS 1: Patches IsValidChatPawn to return true for all pawns.
    // =======================================================================
    [HarmonyPatch]
    public static class HarmonyPatch_PawnChatFilterBypass
    {
        private static readonly Type TargetType = AccessTools.TypeByName("EchoColony.Patch_ChatGizmo");

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (TargetType == null)
            {
                Log.Error("[EchoColonyIntegration] Could not find EchoColony.Patch_ChatGizmo class. Cannot patch IsValidChatPawn.");
                yield break;
            }

            yield return AccessTools.Method(TargetType, "IsValidChatPawn");
        }

        /// <summary>
        /// Transpiler for IsValidChatPawn. Replaces the entire method body to return 'true'.
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_IsValidChatPawn(IEnumerable<CodeInstruction> instructions)
        {
            // Get labels from the start of the original method to attach to our new instruction.
            var startLabels = instructions.FirstOrDefault()?.labels ?? new List<Label>();

            // Ldc_I4_1 (Load integer 1 onto the stack -> true)
            yield return new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(startLabels);
            // Ret (Return)
            yield return new CodeInstruction(OpCodes.Ret);

            Log.Message("[EchoColonyIntegration] Successfully patched EchoColony.IsValidChatPawn to return true.");
        }
    }

    // -----------------------------------------------------------------------

    // =======================================================================
    // CLASS 2: Forces the individual chat gizmo logic to run on ALL selected Pawns.
    // **Group chat logic is entirely excluded.**
    // =======================================================================
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.GetGizmos))]
    public static class HarmonyPatch_UniversalChatGizmo
    {
        private static readonly Type TargetType = AccessTools.TypeByName("EchoColony.Patch_ChatGizmo");

        // Reflection for the necessary methods from the original mod
        private static readonly MethodInfo IsValidChatPawn = AccessTools.Method(TargetType, "IsValidChatPawn");
        private static readonly MethodInfo AreComponentsInitialized = AccessTools.Method(TargetType, "AreComponentsInitialized");
        private static readonly MethodInfo CreateIndividualChatGizmo = AccessTools.Method(TargetType, "CreateIndividualChatGizmo");

        /// <summary>
        /// The Postfix runs for ANY pawn when their gizmos are requested.
        /// Fixes the 'cannot yield a value in a try block' error by collecting gizmos.
        /// </summary>
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            // First, yield all original gizmos
            foreach (Gizmo item in __result)
            {
                yield return item;
            }

            // Gizmo collection list MUST be declared outside the try block
            List<Gizmo> newGizmos = new List<Gizmo>();

            if (TargetType == null)
            {
                goto YieldGizmos; // Skip the try-catch if TargetType is null
            }

            Pawn pawn = __instance;

            try
            {
                // 1. Validity Check (uses the now-patched IsValidChatPawn)
                if (pawn == null || pawn.Map == null || !pawn.Spawned || !(bool)IsValidChatPawn.Invoke(null, new object[] { pawn }))
                {
                    goto YieldGizmos; // Skip to yielding the empty list
                }

                // 2. Components Initialization Check
                if (!(bool)AreComponentsInitialized.Invoke(null, null))
                {
                    goto YieldGizmos;
                }

                // 3. Create Individual Chat Gizmo (Your primary goal)
                Gizmo individualGizmo = (Gizmo)CreateIndividualChatGizmo.Invoke(null, new object[] { pawn });

                if (individualGizmo != null)
                {
                    // ADD the gizmo to the list instead of yielding
                    newGizmos.Add(individualGizmo);
                }

                // IMPORTANT: All group chat logic is intentionally excluded.
            }
            catch (Exception ex)
            {
                Log.Error($"[EchoColonyIntegration] Error injecting universal chat gizmo for {pawn?.LabelShort}: {ex.Message + "\n" + ex.StackTrace}");
            }

        // Yield the collected gizmos OUTSIDE the try-catch block
        YieldGizmos:
            foreach (Gizmo item in newGizmos)
            {
                yield return item;
            }
        }
    }
}