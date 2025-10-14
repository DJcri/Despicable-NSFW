using Despicable;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

/// <summary>
/// Functions for special chat interactions that don't have corresponding 
/// basegame interactions.
/// </summary>
namespace EchoColonyIntegration
{
    public static class SpecialInteractionUtility
    {
        // Handle a recruitment attempt interaction
        // Function takes in success bool from AI response
        public static void HandleRecruitmentAttempt(Pawn heroPawn, Pawn pawnInteractingWith, bool success)
        {
            if (success)
            {
                // If successful, attempt to recruit the pawn
                if (pawnInteractingWith.Faction != Faction.OfPlayer)
                {
                    RecruitUtility.Recruit(pawnInteractingWith, heroPawn.Faction, heroPawn);

                    // Notify the player of the successful recruitment
                    string label = "LetterLabelPawnConvincedToJoin".Translate();
                    string text = "LetterPawnConvincedToJoin".Translate(pawnInteractingWith.LabelShort, heroPawn.LabelShort)
                                  .CapitalizeFirst() + ".";

                    Letter letter = LetterMaker.MakeLetter(
                        label,
                        text,
                        LetterDefOf.PositiveEvent,
                        pawnInteractingWith // Target the recruited pawn for the map view/jump
                    );

                    Find.LetterStack.ReceiveLetter(letter);
                }
            }
        }

        // Handle an attempt to convince a raider to end the raid
        public static void HandleConvinceToEndRaidAttempt(Pawn heroPawn, Pawn pawnInteractingWith, bool success)
        {
            if (success)
            {
                // If successful, end the raid
                if (pawnInteractingWith.Faction != Faction.OfPlayer && pawnInteractingWith.HostileTo(Faction.OfPlayer))
                {
                    Map map = heroPawn.Map;
                    if (map != null)
                    {
                        CommonUtil.EndRaid(map);

                        // Notify the player of the successful negotiation
                        string label = "LetterLabelRaidAverted".Translate();
                        string text = "LetterRaidAverted".Translate(pawnInteractingWith.LabelShort, heroPawn.LabelShort)
                                      .CapitalizeFirst() + ".";

                        Letter letter = LetterMaker.MakeLetter(
                            label,
                            text,
                            LetterDefOf.PositiveEvent,
                            pawnInteractingWith // Target the recruited pawn for the map view/jump
                        );

                        Find.LetterStack.ReceiveLetter(letter);
                    }
                }
            }
        }
    }
}
