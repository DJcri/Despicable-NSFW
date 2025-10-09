using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EchoColonyIntegration
{
    public static class InteractionUtil
    {
        public override void SimulateRomanceAttempt(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            float num = AcceptanceChance(initiator, recipient);
            bool flag = Rand.Value < num;
            bool flag2 = false;
            if (flag)
            {
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.Fiance, recipient);
                if (recipient.needs.mood != null)
                {
                    recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, initiator);
                    recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposalMood, initiator);
                    recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, initiator);
                }
                if (initiator.needs.mood != null)
                {
                    initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposalMood, recipient);
                    initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, recipient);
                    initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, recipient);
                }
                extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalAccepted);
            }
            else
            {
                initiator.needs.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.RejectedMyProposal, recipient);
                recipient.needs.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.IRejectedTheirProposal, initiator);
                extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejected);
                if (Rand.Value < 0.4f)
                {
                    initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                    initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
                    flag2 = true;
                    extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejectedBrokeUp);
                }
            }
            if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (flag)
                {
                    letterLabel = "LetterLabelAcceptedProposal".Translate();
                    letterDef = LetterDefOf.PositiveEvent;
                    stringBuilder.AppendLine("LetterAcceptedProposal".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT")));
                    if (initiator.relations.nextMarriageNameChange != MarriageNameChange.NoChange)
                    {
                        SpouseRelationUtility.DetermineManAndWomanSpouses(initiator, recipient, out var man, out var woman);
                        stringBuilder.AppendLine();
                        if (initiator.relations.nextMarriageNameChange == MarriageNameChange.MansName)
                        {
                            stringBuilder.AppendLine("LetterAcceptedProposal_NameChange".Translate(woman.Named("PAWN"), (man.Name as NameTriple).Last));
                        }
                        else
                        {
                            stringBuilder.AppendLine("LetterAcceptedProposal_NameChange".Translate(man.Named("PAWN"), (woman.Name as NameTriple).Last));
                        }
                    }
                }
                else
                {
                    letterLabel = "LetterLabelRejectedProposal".Translate();
                    letterDef = LetterDefOf.NegativeEvent;
                    stringBuilder.AppendLine("LetterRejectedProposal".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT")));
                    if (flag2)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("LetterNoLongerLovers".Translate(initiator.Named("PAWN1"), recipient.Named("PAWN2")));
                    }
                }
                letterText = stringBuilder.ToString().TrimEndNewlines();
                lookTargets = new LookTargets(initiator, recipient);
            }
            else
            {
                letterLabel = null;
                letterText = null;
                letterDef = null;
                lookTargets = null;
            }
        }
    }
}
