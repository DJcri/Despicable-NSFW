using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Despicable
{
    /// <summary>
    /// STORES DATA pertaining to KARMA and DEEDS
    /// of the player hero, as well as handling of karma abilities
    /// that are based on taking damage
    /// </summary>
    public class CompHero : ThingComp
    {
        public bool isHero = false;
        public float karma = 0f;

        // Deed counter
        public Dictionary<string, int> deedCountValues = new Dictionary<string, int>();
        // Instantiate deed counters
        public override void Initialize(CompProperties props)
        {
            if (deedCountValues.NullOrEmpty())
            {
                deedCountValues = new Dictionary<string, int>();
                foreach (var key in KarmaUtil.karmaPerDeedValues.Keys)
                {
                    deedCountValues.Add(key, 0);
                }
            }

            base.Initialize(props);
        }

        public Pawn pawn => base.parent as Pawn;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref isHero, "isHero", false);
            Scribe_Values.Look(ref karma, "karma", 0f);
            Scribe_Collections.Look(ref deedCountValues, "deedCountValues", LookMode.Value, LookMode.Value);
            base.PostExposeData();
        }

        public Dictionary<string, int> GetDeeds()
        {
            return deedCountValues;
        }

        // Handle damage here, instead of creating a patch for this module
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            if (!pawn.IsPrisoner)
                return;

            Thing instigator = dinfo.Instigator;

            // Functionality for passive karmaic abilities that involve damaging pawns
            if (instigator is Pawn)
            {
                CompHero heroComp = instigator.TryGetComp<CompHero>();
                if (heroComp == null || !heroComp.isHero)
                    return;

                if (KarmaUtil.IsAbilityActive(HeroModule_KarmaicAbilityDefOf.KarmaicAbility_Torture, instigator as Pawn))
                {
                    pawn.guest.will -= 0.1f;
                }

                // For ability "Fear Monger"
                if (KarmaUtil.IsAbilityActive(HeroModule_KarmaicAbilityDefOf.KarmaicAbility_FearMonger, instigator as Pawn)
                    && pawn.Faction != instigator.Faction
                    && pawn.health.Dead)
                {
                    int roll = Rand.Range(0, 99);

                    if (roll < HeroUtil.fearMongerChance)
                        pawn.GetLord().GotoToil(new LordToil_ExitMapFighting());
                }
            }

            // If hero dies, toggle isHero, so player can switch heroes
            if (isHero && pawn.Dead)
            {
                isHero = false;
                foreach (var ability in pawn.abilities.AllAbilitiesForReading.ToList())
                {
                    if (ability.def.comps[0].GetType() == typeof(CompProperties_AbilityKarmaic))
                    {
                        pawn.abilities.RemoveAbility(ability.def);
                    }
                }
            }
        }
    }
}