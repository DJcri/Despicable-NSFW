using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Despicable
{
    public class IntimacyIntegration : Mod
    {
        public IntimacyIntegration(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Despicable.IntimacyIntegration");
            harmony.PatchAll();
        }
    }

    // Use Intimacy's checks instead for cleaner integration
    [HarmonyPatch(typeof(LovinUtil), "PassesLovinCheck")]
    static class HarmonyPatch_LovinUtil
    {
        public static void Postfix(ref bool __result, ref Pawn pawn, ref Pawn target)
        {
            __result = true;

            if (!SexUtilities.CanDoLovin(pawn) || !SexUtilities.CanDoLovin(target)
                || (!SocialInteractionUtility.CanInitiateInteraction(pawn))
                || (!CommonChecks.IsOldEnough(pawn) || !CommonChecks.IsOldEnough(target))
                || (!CommonChecks.AreMutuallyAttracted(pawn, target))
                || (CommonChecks.IdeologyForbidsLovin(pawn, target))
                || (CommonChecks.IncestCheck(pawn, target)))
            {
                __result = false;
            }
        }
    }

    // Use Intimacy's checks to determine why pawns can't do lovin
    [HarmonyPatch(typeof(LovinUtil), "GetReportForLovin")]
    static class HarmonyPatch_LovinReport
    {
        public static void Postfix(ref string __result, ref Pawn pawn, ref Pawn pawn2)
        {
            // CanDoLovin not used here, since this function would only be used if
            // it didn't pass in the first place

            __result = "[Intimacy] Pawn drafted or lovin' on cooldown";
            if (!SocialInteractionUtility.CanInitiateInteraction(pawn))
            {
                __result = "[Intimacy] Pawn can't initiate interaction due to condition or sleeping";
            }
            if (!CommonChecks.IsOldEnough(pawn) || !CommonChecks.IsOldEnough(pawn2))
            {
                __result = "[Intimacy] Pawn isn't old enough";
            }
            if (!CommonChecks.AreMutuallyAttracted(pawn, pawn2))
            {
                __result = "[Intimacy] Pawn's aren't attracted to each other";
            }
            if (CommonChecks.IdeologyForbidsLovin(pawn, pawn2))
            {
                __result = "[Intimacy] Ideology forbids lovin'";
            }
            if (CommonChecks.IncestCheck(pawn, pawn2))
            {
                __result = "[Intimacy] Incest not desired by these pawns";
            }
        }
    }

    // Play a random sex animation if sex job driver from Turkler's "Intimacy Mod" is being used
    [HarmonyPatch(typeof(JobDriver_Sex), "BothPawnsAreHavingSex")]
    static class HarmonyPatch_JobDriver_SexStartAnim
    {
        public static void Postfix(ref bool __result, Pawn pawn, Pawn partner)
        {
            if (__result)
            {
                // Set participants
                List<Pawn> participants = new List<Pawn>()
                {
                    pawn,
                    partner
                };
                // Gain intimacy from lovin'
                // Recreation and rest depletion are already handled
                // in the base mod
                CommonChecks.TryGainIntimacy(pawn, CommonChecks.IntimacyFromLovin);

                if (CommonUtil.GetSettings().animationExtensionEnabled)
                {
                    // Check if anim is already playing, so animations don't overlap
                    CompExtendedAnimator extAnimator = pawn.TryGetComp<CompExtendedAnimator>();
                    if (extAnimator != null)
                    {
                        if (extAnimator.hasAnimPlaying)
                            return;
                    }

                    if (pawn.pather.Moving || partner.pather.Moving)
                        return;

                    extAnimator = partner.TryGetComp<CompExtendedAnimator>();
                    if (extAnimator != null)
                        if (extAnimator.hasAnimPlaying) return;

                    // Get a random animation, using a random lovin' type for pawns to play
                    List<AnimGroupDef> playableAnimations = ContextUtil.GetPlayableAnimationsFor(participants);

                    if (!playableAnimations.NullOrEmpty())
                    {
                        AnimGroupDef animGroupDef = playableAnimations.RandomElement();
                        Dictionary<string, Pawn> roleAssignments = ContextUtil.AssignRoles(animGroupDef, participants);


                        if (roleAssignments != null)
                        {
                            AnimUtil.PlayAnimationGroup(animGroupDef, roleAssignments, pawn);
                            pawn.jobs.curDriver.AddFinishAction(c => {
                                AnimUtil.ResetAnimatorsForGroup(participants);
                            });
                        }
                    }
                }
            }
        }
    }
}
