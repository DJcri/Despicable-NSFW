using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace Despicable
{
    /// <summary>
    /// Internal configuration for lovin' extensions AND
    /// handles RESTRICTIONS, plus LOGIC for SEXUAL INTERACTIONS
    /// </summary>
    public static class LovinUtil
    {
        // Use duration ticks if no animations found
        public static int defaultDurationTicks = 10000;
        public static float lovinMaxPainThreshold = 0.6f;
        public static float lovinMinCompatibility = -1;
        public static int lovinMinOpinion = 20;
        public static int maxLovinPartners = 3;
        public static int ticksBetweenHearts = 120;
        public static int restDepletionInterval = 96;

        public static Pawn FindPartner(Pawn pawn, bool inBed = false)
        {
            Pawn lover = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
            Pawn spouse = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
            Pawn partner = null;
            Map map = pawn.Map;

            if (lover == null && spouse == null)
            {
                List<Pawn> targets = map.mapPawns.AllPawns.Where(target
                    => PassesLovinCheck(pawn, target)
                    && ((!inBed && !target.InBed()) || (inBed && (AloneInBed(target) || InSameBed(target, pawn))))
                    ).ToList();

                if (targets.Count > 0)
                {
                    foreach (var potentialPartner in targets)
                    {
                        if (partner == null 
                            || pawn.relations.OpinionOf(potentialPartner) > pawn.relations.OpinionOf(partner)
                            )
                        {
                            partner = potentialPartner;
                        }
                    }
                }
            }
            else
            {
                partner = spouse == null ? lover : spouse;
                if (partner.Map != pawn.Map)
                {
                    return null;
                }
            }

            return partner;
        }

        /// <summary>
        /// Checks whether or not the pawn would like to participate in lovin'
        /// If the lovin was ordered, ignore check for recreation or recent lovin'
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="orderedLovin"></param>
        /// <returns></returns>
        public static bool CouldUseSomeLovin(Pawn pawn, bool orderedLovin = false)
        {
            // Don't look for lovin' in these conditions
            if (pawn == null)
                return false;
            // If not humanlike
            if (!pawn.RaceProps?.Humanlike == true)
                return false;
            // If underage
            if (!pawn.ageTracker?.Adult == true || (pawn.ageTracker?.AgeChronologicalYears < 18) || (pawn.ageTracker?.AgeBiologicalYears < 18))
                return false;
            // If drafted
            if (pawn.Drafted)
                return false;
            // If pawn is travelling or leaving
            if (pawn.mindState?.duty?.def == DutyDefOf.TravelOrLeave)
                return false;
            // Ignore these conditions if lovin' was ordered
            if (!orderedLovin && !CommonUtil.GetSettings().debugMode)
            {
                // If pawn recently had lovin'
                if (pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(ThoughtDefOf.GotSomeLovin) != null)
                    return false;
                // If pawn's recreation is low enough
                if (!(pawn.needs?.joy?.CurLevelPercentage < 0.6f))
                    return false;
            }

            return true;
        }

        public static bool PassesLovinCheck(Pawn pawn, Pawn target, bool ordered = false)
        {
            if (target != pawn
                && CouldUseSomeLovin(pawn, ordered)
                && CouldUseSomeLovin(target, ordered)
                && PassesHealthCheck(pawn)
                && PassesHealthCheck(target)
                && PassesOrientationCheck(pawn, target)
                && PassesRelationsCheck(pawn, target)
                && PassesIdeologyCheck(pawn, target)
                )
            {
                return true;
            }

            return false;
        }

        public static bool PassesIdeologyCheck(Pawn pawn, Pawn target)
        {
            if (ModLister.IdeologyInstalled)
            {
                if (pawn == null)
                    return false;
                if (CommonUtil.GetSettings().debugMode)
                    return true;

                bool spouseOnly = pawn.Ideo?.GetPrecept(DefDatabase<PreceptDef>.GetNamed("Lovin_SpouseOnly_Strict")) != null;
                bool freeLovin = pawn.Ideo?.GetPrecept(DefDatabase<PreceptDef>.GetNamed("Lovin_FreeApproved")) != null;
                bool lovinHorrible = pawn.Ideo?.GetPrecept(DefDatabase<PreceptDef>.GetNamed("Lovin_Horrible")) != null;

                // No lovin if prudish
                // or not married and ideo is strict
                if (lovinHorrible
                    || (spouseOnly && !pawn.relations?.DirectRelationExists(PawnRelationDefOf.Spouse, target) == true))
                {
                    return false;
                }

                if (freeLovin)
                    return true;

                // Additional logic here, not overridden by main precepts

            }

            return true;
        }

        public static bool PassesRelationsCheck(Pawn pawn, Pawn target)
        {
            if (pawn == null)
                return false;
            if (CommonUtil.GetSettings().debugMode)
                return true;

            // Check for relationship and compatibility
            // If ideology not installed, ensure no cheating
            // Otherwise, respect precepts
            if (pawn.relations?.DirectRelationExists(PawnRelationDefOf.Lover, target) == true
                || pawn.relations?.DirectRelationExists(PawnRelationDefOf.Spouse, target) == true)
            {
                return true;
            }
            // Ignore opinion and compatibility if already in partnership with each other,
            // Otherwise, they may never have sex
            if (!(pawn.relations?.OpinionOf(target) >= lovinMinOpinion))
            {
                return false;
            }
            if (!(pawn.relations?.CompatibilityWith(target) >= lovinMinCompatibility))
            {
                return false;
            }

            return true;
        }

        public static bool PassesOrientationCheck(Pawn pawn, Pawn target)
        {
            if (pawn == null)
                return false;
            if (CommonUtil.GetSettings().debugMode)
                return true;

            Gender pawnGender = pawn.gender;
            bool gay = pawn.story?.traits?.HasTrait(TraitDefOf.Gay) == true;
            bool bisexual = pawn.story?.traits?.HasTrait(TraitDefOf.Bisexual) == true;
            
            if (gay && !(PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)target.gender)))
            {
                return false;
            }
            if (!bisexual && !gay && PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)target.gender))
            {
                return false;
            }

            return true;
        }

        public static bool PassesHealthCheck(Pawn pawn)
        {
            if (pawn == null)
                return false;

            if (pawn.health?.capacities?.CanBeAwake == true
                && pawn.health?.hediffSet?.BleedRateTotal <= 0.0f
                && pawn.health?.hediffSet?.PainTotal <= lovinMaxPainThreshold
                && !pawn.Dead == true
                && !pawn.needs?.food?.Starving == true
                )
            {
                return true;
            }

            return false;
        }

        public static bool InSameBed(Pawn pawn, Pawn partner)
        {
            if (pawn.InBed() && partner.InBed())
            {
                if (pawn.CurrentBed() == partner.CurrentBed())
                    return true;
            }
            return false;
        }

        public static bool AloneInBed(Pawn pawn)
        {
            if (pawn.CurrentBed().CurOccupants.Count() == 1)
            {
                return true;
            }
            return false;
        }

        public static bool IsLovin(Pawn pawn)
        {
            if (pawn == null)
                return false;

            if (pawn.CurJobDef == JobDefOf.Lovin
                || pawn.CurJobDef == LovinModule_JobDefOf.Job_GiveLovin
                || pawn.CurJobDef == LovinModule_JobDefOf.Job_GetLovin
                || pawn.CurJobDef == LovinModule_JobDefOf.Job_GetBedLovin)
                return true;
            return false;
        }

        public static string GetReportForLovin(Pawn pawn, Pawn pawn2)
        {
            if (pawn == null)
                return "Pawn doesn't exist";

            string healthCheckReport = " is not healthy";
            string ideoCheckReport = "Ideo forbids one or more pawns";
            string orientationCheckReport = "Sexually incompatible";
            string relationsCheckReport = "Not enough attraction";

            if (!PassesHealthCheck(pawn))
                return pawn.Name.ToStringShort + healthCheckReport;
            if (!PassesHealthCheck(pawn2))
                return pawn.Name.ToStringShort + healthCheckReport;
            if (!PassesIdeologyCheck(pawn, pawn2))
                return ideoCheckReport;
            if (!PassesOrientationCheck(pawn, pawn2))
                return orientationCheckReport;
            if (!PassesRelationsCheck(pawn, pawn2))
                return relationsCheckReport;

            return null;
        }
    }
}
