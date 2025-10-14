using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using EchoColony;

namespace EchoColonyIntegration
{
    [HarmonyPatch(typeof(ColonistPromptContextBuilder), "BuildSystemPrompt")]
    public static class HarmonyPatch_ReviseSystemPrompt
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref string __result)
        {
            // 1. Get Pawn Info (Non-Redundant)
            string fullName = pawn.Label;
            string genderLabel = pawn.gender.ToString();
            string xenotypeLabel = pawn.genes?.Xenotype?.label ?? "baseline human";
            string factionName = pawn.Faction?.Name ?? "wild pawn"; // Use pawn.Faction
            string technologyLevel = pawn.Faction?.def?.techLevel.ToString() ?? "unknown tech level";
            string settlementName = Find.CurrentMap.info?.parent?.LabelCap ?? "unknown settlement";

            // 2. Determine Alignment (Strict Hierarchy)
            string alignmentLabel;

            // Colony Status (Overrides Faction status)
            if (pawn.IsPrisonerOfColony)
            {
                alignmentLabel = "prisoner";
            }
            else if (pawn.IsSlaveOfColony)
            {
                alignmentLabel = "slave";
            }
            else if (pawn.Faction == Faction.OfPlayer)
            {
                alignmentLabel = "colonist"; // Colony member (must be checked after prisoner/slave)
            }
            // External Factions
            else if (pawn.Faction != null)
            {
                if (pawn.Faction.HostileTo(Faction.OfPlayer))
                {
                    // More specific hostile faction check is generally cleaner for AI prompts
                    if (pawn.Faction == Faction.OfInsects)
                    {
                        alignmentLabel = "enemy insectoid";
                    }
                    else if (pawn.Faction == Faction.OfMechanoids)
                    {
                        alignmentLabel = "enemy mechanoid";
                    }
                    else if (pawn.Faction == Faction.OfPirates)
                    {
                        alignmentLabel = "pirate raider"; // Changed to be more descriptive
                    }
                    else
                    {
                        alignmentLabel = "hostile enemy"; // Generic hostile faction
                    }
                }
                else if (pawn.Faction.AllyOrNeutralTo(Faction.OfPlayer))
                {
                    // TradersGuild is just a specific kind of neutral/allied visitor
                    if (pawn.Faction == Faction.OfTradersGuild)
                    {
                        alignmentLabel = "trader's guild member";
                    }
                    else if (pawn.Faction == Faction.OfAncients)
                    {
                        alignmentLabel = "ancient visitor";
                    }
                    else
                    {
                        alignmentLabel = "visitor"; // Allied or neutral faction
                    }
                }
                else
                {
                    alignmentLabel = "visitor"; // Unspecified or neutral faction
                }
            }
            // Wild or Unfactioned Pawn (Default fallback)
            else
            {
                alignmentLabel = "solo traveler";
            }

            // 3. Construct the Final Prompt
            __result = $"You are {fullName}, a {technologyLevel} {alignmentLabel} in RimWorld.\n" +
                       $"You identify as {genderLabel} (Xenotype: {xenotypeLabel}).\n" +
                       $"You belong to the faction \"{factionName}\".\n" +
                       $"You are currently in the {settlementName}.\n" +
                       "**Speak from your perspective and stay in character**";
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