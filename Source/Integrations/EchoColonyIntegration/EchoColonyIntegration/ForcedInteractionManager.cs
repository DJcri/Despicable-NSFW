
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Despicable
{
    public static class ForcedInteractionManager
    {
        // This is the new method that will replace TryInteractWith.
        public static void SimulateInteraction(Pawn initiator, Pawn recipient, string interactionDefName, int relationshipChange, string moodEffect)
        {
            // 1. Get the Interaction Definition
            InteractionDef interactionDef = DefDatabase<InteractionDef>.GetNamed(interactionDefName, false);
            if (interactionDef == null)
            {
                Log.Error($"[Despicable] - Could not find InteractionDef named {interactionDefName}. Aborting interaction.");
                return;
            }

            // 2. Apply the main opinion change
            // This is the most direct and reliable way to change opinions.
            // It also creates a log entry for the opinion change reason.
            recipient.relations.ChangeOpinion(initiator, relationshipChange * EchoColonyIntegration.OPINION_CHANGE_MULTIPLIER, interactionDef.logRulesInitiator);

            // 3. Apply the custom mood effect from the AI
            ThoughtDef moodEffectDef = DefDatabase<ThoughtDef>.GetNamed(moodEffect + "MoodEffect", false);
            if (moodEffectDef != null)
            {
                recipient.needs.mood?.thoughts?.memories?.TryGainMemory(moodEffectDef, initiator);
            }

            // 4. Handle special interaction logic
            // This is where we manually replicate the outcomes of special interactions.
            HandleSpecialInteractionOutcomes(initiator, recipient, interactionDef, relationshipChange > 0);

            // 5. Add an entry to the social log so it appears in the UI
            // This makes the game recognize that an interaction happened for the log history.
            Find.PlayLog.Add(new PlayLogEntry_Interaction(interactionDef, initiator, recipient, null));
        }

        private static void HandleSpecialInteractionOutcomes(Pawn initiator, Pawn recipient, InteractionDef intDef, bool wasSuccessful)
        {
            // --- Romance Attempt ---
            if (intDef.Worker is InteractionWorker_RomanceAttempt)
            {
                if (wasSuccessful)
                {
                    // Manually make them lovers
                    initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
                    // Add the "New lover" thought
                    initiator.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.GotSomeLovin, recipient);
                    recipient.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.GotSomeLovin, initiator);
                }
                else
                {
                    // Add the "Rebuffed" thought to the initiator
                    initiator.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
                }
            }
            // --- Marriage Proposal ---
            else if (intDef.Worker is InteractionWorker_MarriageProposal)
            {
                if (wasSuccessful)
                {
                    // Manually make them fiances
                    initiator.relations.AddDirectRelation(PawnRelationDefOf.Fiance, recipient);
                }
                else
                {
                    // Add negative thoughts
                    initiator.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.RejectedMyProposal, recipient);
                    recipient.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.IRejectedTheirProposal, initiator);
                }
            }
            // --- Breakup ---
            else if (intDef.Worker is InteractionWorker_Breakup)
            {
                // Remove lover/fiance/spouse relation regardless of "success"
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
                // Add the "Broke up" thoughts
                recipient.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.BrokeUpWithMe, initiator);
            }
            // Add other special cases like Insults here if needed.
            // For most simple interactions, the opinion change and log entry are enough.
        }
    }
}