using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace Despicable
{
    public class CompAbilityEffect_Karmaic : CompAbilityEffect
    {
        public new CompProperties_AbilityKarmaic Props => (CompProperties_AbilityKarmaic)props;

        public override bool HideTargetPawnTooltip => true;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = parent.pawn;
            Pawn pawn2 = target.Pawn;
            AbilityDef abilityDef = parent.def;
            KarmaicAbilityDef karmaicAbilityDef = (abilityDef.comps[0] as CompProperties_AbilityKarmaic).karmaicAbilityDef;
            PlayLogEntry_Interaction entry = null;

            switch (parent.def.defName)
            {
                case "PetAnimal": // Creates a bond with the animal
                    pawn2.relations.AddDirectRelation(PawnRelationDefOf.Bond, pawn);
                    entry = new PlayLogEntry_Interaction(HeroModule_InteractionDefOf.PetAnimal, parent.pawn, pawn2, null);
                    break;
                case "OperantTraining": // Trains animal instantly
                    pawn2.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1f, 0f, -1, pawn));
                    TrainableDef nextTrainable = pawn2.training.NextTrainableToTrain();
                    if (nextTrainable != null)
                    {
                        pawn2.training.Train(nextTrainable, pawn, true);
                        entry = new PlayLogEntry_Interaction(HeroModule_InteractionDefOf.OperantTraining, parent.pawn, pawn2, null);
                    }
                    break;
            }

            if (entry != null)
                Find.PlayLog.Add(entry);

            if (Props.sound != null)
                Props.sound.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null)
                return false;

            AbilityDef abilityDef = parent.def;
            KarmaicAbilityDef karmaicAbilityDef = (abilityDef.comps[0] as CompProperties_AbilityKarmaic).karmaicAbilityDef;

            if (karmaicAbilityDef == HeroModule_KarmaicAbilityDefOf.KarmaicAbility_PetAnimal
                || karmaicAbilityDef == HeroModule_KarmaicAbilityDefOf.KarmaicAbility_OperantTraining)
            {
                if (!AbilityUtility.ValidateMustBeAnimal(pawn, throwMessages, parent))
                    return false;
                if (target.Pawn.Faction != pawn.Faction)
                    return false;

                if (karmaicAbilityDef == HeroModule_KarmaicAbilityDefOf.KarmaicAbility_OperantTraining)
                {
                    TrainableDef nextTrainable = target.Pawn.training.NextTrainableToTrain();
                    if (nextTrainable == null || !(target.Pawn.Faction == pawn.Faction))
                        return false;
                }
                else /// if ability is pet animal
                {
                    List<Pawn> bonds = new List<Pawn>();
                    target.Pawn.relations.GetDirectRelations(PawnRelationDefOf.Bond, ref bonds);
                    if (bonds.Count > 0)
                        return false;

                    target.Pawn.relations.AddDirectRelation(PawnRelationDefOf.Bond, pawn);
                    FleckMaker.ThrowMetaIcon(target.Pawn.Position, target.Pawn.Map, FleckDefOf.Heart);
                }
            }

            /// Keeping this here in case I want to make other abilities later
            /*
            if (!AbilityUtility.ValidateMustBeHuman(pawn, throwMessages, parent))
            {
                return false;
            }
            if (!AbilityUtility.ValidateMustNotBeBaby(pawn, throwMessages, parent))
            {
                return false;
            }
            if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
            {
                return false;
            }
            if (!AbilityUtility.ValidateNotSameIdeo(parent.pawn, pawn, throwMessages, parent))
            {
                return false;
            }
            if (!AbilityUtility.ValidateIsConscious(pawn, throwMessages, parent))
            {
                return false;
            }
            */

            return true;
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            AbilityDef abilityDef = parent.def;
            KarmaicAbilityDef karmaicAbilityDef = (abilityDef.comps[0] as CompProperties_AbilityKarmaic).karmaicAbilityDef;

            if (target.Pawn == null || !Valid(target))
            {
                return null;
            }

            Pawn pawn = parent.pawn;
            Pawn pawn2 = target.Pawn;

            return karmaicAbilityDef.description.Translate();
        }
    }
}
