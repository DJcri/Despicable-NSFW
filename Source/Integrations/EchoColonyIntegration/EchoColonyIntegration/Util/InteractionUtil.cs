using Despicable;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace EchoColonyIntegration
{
    /// <summary>
    /// Handles the game logic consequences for special chat interactions that don't have corresponding 
    /// basegame InteractionDefs.
    /// </summary>
    public static class InteractionUtil
    {
        /// <summary>
        /// Handles the game-world effects of a special interaction based on the success flag from the AI response.
        /// </summary>
        /// <param name="heroPawn">The pawn initiating the interaction (the player's pawn).</param>
        /// <param name="pawnInteractingWith">The target pawn.</param>
        /// <param name="interactionLabel">The label from the AI's JSON response (e.g., "RecruitmentAttempt").</param>
        /// <param name="success">The success bool from the AI's JSON response.</param>
        public static void HandleSpecialInteraction(Pawn heroPawn, Pawn pawnInteractingWith, string interactionLabel, bool success)
        {
            if (!success)
            {
                // If the AI flagged it as a failure, no game-changing action is taken.
                // The relationship and mood changes are handled separately by the JSON parser.
                return;
            }

            // Only proceed if the success flag is true
            switch (interactionLabel)
            {
                case "RecruitmentAttempt":
                    // Handle a recruitment attempt interaction
                    if (pawnInteractingWith.Faction != Faction.OfPlayer)
                    {
                        RecruitUtility.Recruit(pawnInteractingWith, heroPawn.Faction, heroPawn);

                        // Notify the player of the successful recruitment
                        string label = "LetterLabelPawnConvincedToJoin".Translate();
                        string text = "LetterPawnConvincedToJoin".Translate(pawnInteractingWith.LabelShort, heroPawn.LabelShort).CapitalizeFirst() + ".";

                        Letter letter = LetterMaker.MakeLetter(
                            label,
                            text,
                            LetterDefOf.PositiveEvent,
                            pawnInteractingWith // Target the recruited pawn
                        );
                        Find.LetterStack.ReceiveLetter(letter);
                    }
                    break;

                case "ConvinceToEndRaidAttempt":
                    // Handle an attempt to convince a raider to end the raid
                    if (pawnInteractingWith.Faction != Faction.OfPlayer && pawnInteractingWith.HostileTo(Faction.OfPlayer))
                    {
                        Map map = heroPawn.Map;
                        if (map != null)
                        {
                            // Assuming CommonUtil.EndRaid is a valid function that handles raid ending logic
                            CommonUtil.EndRaid(map);

                            // Notify the player of the successful negotiation
                            string label = "LetterLabelRaidAverted".Translate();
                            string text = "LetterRaidAverted".Translate(pawnInteractingWith.LabelShort, heroPawn.LabelShort).CapitalizeFirst() + ".";

                            Letter letter = LetterMaker.MakeLetter(
                                label,
                                text,
                                LetterDefOf.PositiveEvent,
                                pawnInteractingWith
                            );
                            Find.LetterStack.ReceiveLetter(letter);
                        }
                    }
                    break;

                case "SnapOutOfMentalBreakAttempt":
                    // Handle an attempt to snap a pawn out of a mental break
                    if (pawnInteractingWith.InMentalState)
                    {
                        pawnInteractingWith.mindState.mentalStateHandler.CurState.RecoverFromState();
                        // Optionally notify the player of the successful intervention
                        Messages.Message($"{heroPawn.LabelShort} has successfully helped {pawnInteractingWith.LabelShort} snap out of their mental break.", MessageTypeDefOf.PositiveEvent);
                    }
                    break;

                // Add other special interactions here as you define them (e.g., "MarriageProposal", "Breakup")

                default:
                    // Optionally log an error if an unhandled special interaction label is received
                    Log.Warning($"EchoColony: Received unhandled special interaction label: {interactionLabel}");
                    break;
            }
        }
    }
}