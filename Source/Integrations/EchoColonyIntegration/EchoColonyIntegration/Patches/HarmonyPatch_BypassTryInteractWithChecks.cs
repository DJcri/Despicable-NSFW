using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EchoColonyIntegration.Patches
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
    public static class HarmonyPatch_BypassTryInteractWithChecks
    {
        // Method that checks the call stack to see if the interaction is initiated by our mod.
        public static bool ShouldBypassChecks()
        {
            // Note: Stack walking can be slow, but it's the intended logic here.
            var stackTrace = new StackTrace();

            // Start at 1 to skip the current method (ShouldBypassChecks) itself.
            for (var i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                MethodBase method = frame?.GetMethod();
                string callingNamespace = method?.DeclaringType?.Namespace;

                if (callingNamespace == "Despicable")
                {
                    return true;
                }
            }

            return false;
        }

        // Corrected Transpiler patch that bypasses interaction checks if called from our mod.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeMatcher = new CodeMatcher(instructions, il);

            // --- PATCH 1: 'CanInteractNowWith' ---
            codeMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.CanInteractNowWith)))
            );

            // CRITICAL FIX: Check if the match succeeded (.IsValid) before trying to access instructions.
            // This prevents the ArgumentOutOfRangeException.
            if (codeMatcher.IsValid)
            {
                // The jump instruction (Brfalse.S) is immediately after the Callvirt.
                // Move the matcher to the jump instruction.
                codeMatcher.Advance(1);

                // Now, InstructionAt(0) is the jump instruction, so we safely get its operand (the jump target).
                var jumpTargetLabel = codeMatcher.InstructionAt(0).operand;

                // Move back to insert our check BEFORE the original check.
                codeMatcher.Insert(
                    // Call our helper method.
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_BypassTryInteractWithChecks), nameof(ShouldBypassChecks))),
                    // If it returns true, jump over the original check entirely.
                    new CodeInstruction(OpCodes.Brtrue_S, jumpTargetLabel)
                );
            }
            else
            {
                Log.Warning("Harmony patch failed (Patch 1: CanInteractNowWith). Target instruction not found. Transpiler will not be applied.");
                return instructions; // Return original instructions if patch fails
            }

            // --- PATCH 2: 'InteractedTooRecentlyToInteract' ---

            // First, find the jump target for the time check block using our corrected helper.
            object timeCheckJumpTarget;
            try
            {
                timeCheckJumpTarget = FindTimeCheckJumpTarget(instructions);
            }
            catch (HarmonyException ex)
            {
                // If the helper fails, log the error and skip the rest of the patch.
                Log.Warning("Harmony patch failed (Patch 2): " + ex.Message + ". Transpiler will not be applied fully.");
                return codeMatcher.InstructionEnumeration(); // Return partially patched instructions
            }


            // We need to find the start of the whole 'if' block to insert our check at the beginning.
            // The original code loads 'intDef' onto the stack just before the block starts.
            codeMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_2) // ldarg.2 loads 'intDef' onto the stack
            );

            if (codeMatcher.IsValid)
            {
                // Insert our new check at the start of the block.
                codeMatcher.Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_BypassTryInteractWithChecks), nameof(ShouldBypassChecks))),
                    // If our check is true, jump past the original time checks using the target label we found.
                    new CodeInstruction(OpCodes.Brtrue_S, timeCheckJumpTarget)
                );
            }
            else
            {
                Log.Warning("Harmony patch failed (Patch 2: Ldarg_2 start). Target instruction not found. Skipping remaining patch.");
            }


            return codeMatcher.InstructionEnumeration();
        }

        /// <summary>
        /// Helper to find the correct jump target for the time check block.
        /// Finds the branch target that skips the InteractedTooRecentlyToInteract() call.
        /// </summary>
        private static object FindTimeCheckJumpTarget(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(InteractionDef), nameof(InteractionDef.ignoreTimeSinceLastInteraction)))
            );

            if (matcher.IsValid)
            {
                // The instruction immediately after the Ldfld is the conditional jump (Brtrue.s)
                // Move the matcher to the jump instruction itself.
                matcher.Advance(1);

                // Return the operand (Label) of the jump instruction.
                return matcher.InstructionAt(0).operand;
            }

            // If the IL pattern isn't found, throw a specific exception.
            throw new Exception("Target IL pattern for time check jump not found.");
        }
    }
}
