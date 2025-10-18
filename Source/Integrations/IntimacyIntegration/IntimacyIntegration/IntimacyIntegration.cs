using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

/// <summary>
/// Patches animation into lovin' from Turkler's Intimacy mod,
/// as well as replaces lovin' checks with Intimacy's checks.
/// </summary>

namespace Despicable
{
    public class IntimacyIntegration : Mod
    {
        public IntimacyIntegration(ModContentPack content)
            : base(content)
        {
            Harmony val = new Harmony("Despicable.IntimacyIntegration");
            val.PatchAll();
        }
    }

    // Patch to replace lovin' checks with Intimacy's checks
    [HarmonyPatch(typeof(LovinUtil), "PassesLovinCheck")]
    public static class HarmonyPatch_LovinUtil
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, ref Pawn pawn, ref Pawn target, ref bool ordered)
        {
            __result = true;
            if (!LovinUtil.CouldUseSomeLovin(pawn, ordered) || !LovinUtil.CouldUseSomeLovin(target, ordered) || !CustomLovinInitCheck.CanInitiateInteraction(pawn, null) || !CommonChecks.IsOldEnough(pawn) || !CommonChecks.IsOldEnough(target) || !CommonChecks.AreMutuallyAttracted(pawn, target) || CommonChecks.IdeologyForbidsLovin(pawn, target) || CommonChecks.IncestCheck(pawn, target))
            {
                __result = false;
            }
        }
    }

    // Patch to replace lovin' report text to correspond with Intimacy's checks
    [HarmonyPatch(typeof(LovinUtil), "GetReportForLovin")]
    public static class HarmonyPatch_LovinReport
    {
        [HarmonyPostfix]
        public static void Postfix(ref string __result, ref Pawn pawn, ref Pawn pawn2)
        {
            __result = "Pawn is unavailable for lovin'";
            if (!CustomLovinInitCheck.CanInitiateInteraction(pawn, null))
            {
                __result = "Lovin' can't be initiated right now";
            }
            if (!CommonChecks.IsOldEnough(pawn) || !CommonChecks.IsOldEnough(pawn2))
            {
                __result = "Pawn isn't old enough";
            }
            if (!CommonChecks.AreMutuallyAttracted(pawn, pawn2))
            {
                __result = "Pawn's aren't attracted to each other";
            }
            if (CommonChecks.IdeologyForbidsLovin(pawn, pawn2))
            {
                __result = "Ideology forbids lovin'";
            }
            if (CommonChecks.IncestCheck(pawn, pawn2))
            {
                __result = "Incest not desired by these pawns";
            }
        }
    }

    // Patch to play animation when lovin' starts
    [HarmonyPatch(typeof(JobDriver_Sex), "BothPawnsAreHavingSex")]
    public static class HarmonyPatch_JobDriver_SexStartAnim
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn pawn, Pawn partner)
        {
            if (!__result)
            {
                return;
            }
            List<Pawn> participants = new List<Pawn> { pawn, partner };
            CommonChecks.TryGainIntimacy(pawn, CommonChecks.IntimacyFromLovin);
            if (!CommonUtil.GetSettings().animationExtensionEnabled)
            {
                return;
            }
            CompExtendedAnimator val = ThingCompUtility.TryGetComp<CompExtendedAnimator>((Thing)(object)pawn);
            if ((val != null && val.hasAnimPlaying) || pawn.pather.Moving || partner.pather.Moving)
            {
                return;
            }
            val = ThingCompUtility.TryGetComp<CompExtendedAnimator>((Thing)(object)partner);
            if (val != null && val.hasAnimPlaying)
            {
                return;
            }
            List<AnimGroupDef> playableAnimationsFor = ContextUtil.GetPlayableAnimationsFor(participants, (LovinTypeDef)null);
            if (GenList.NullOrEmpty<AnimGroupDef>((IList<AnimGroupDef>)playableAnimationsFor))
            {
                return;
            }
            AnimGroupDef val2 = GenCollection.RandomElement<AnimGroupDef>((IEnumerable<AnimGroupDef>)playableAnimationsFor);
            Dictionary<string, Pawn> dictionary = ContextUtil.AssignRoles(val2, participants);
            if (dictionary != null)
            {
                AnimUtil.PlayAnimationGroup(val2, dictionary, (Thing)(object)pawn);
                pawn.jobs.curDriver.AddFinishAction((Action<JobCondition>)delegate
                {
                    AnimUtil.ResetAnimatorsForGroup(participants);
                });
            }
        }
    }
}
