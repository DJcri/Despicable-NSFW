using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Linq;

namespace Despicable
{
    public class InteractionMenu
    {
        private static TargetingParameters targetParametersCache = null;

        private static TargetingParameters TargetParameters
        {
            get
            {
                if (targetParametersCache == null)
                {
                    targetParametersCache = new TargetingParameters()
                    {
                        canTargetHumans = true,
                        canTargetAnimals = false,
                        canTargetItems = false,
                        mapObjectTargetsMustBeAutoAttackable = false,
                    };
                }
                return targetParametersCache;
            }
        }

        public static void InitInteractionMenu(Pawn pawn, List<FloatMenuOption> opts, Vector3 clickPos)
        {
            if (CommonUtil.GetSettings().lovinExtensionEnabled)
            {
                IEnumerable<LocalTargetInfo> validTargets = GenUI.TargetsAt(clickPos, TargetParameters);

                // The user has clicked on something and the loop is running.
                foreach (LocalTargetInfo target in validTargets)
                {
                    // A quick way to get the pawn if it exists, otherwise it will be null.
                    Pawn targetPawn = null;
                    if (target.Pawn != null)
                    {
                        targetPawn = target.Pawn;

                        // Check if RaceProps exists before using it.
                        if (targetPawn.RaceProps == null || !targetPawn.RaceProps.Humanlike)
                            continue;
                        if (!targetPawn.Spawned)
                            continue;
                        if (targetPawn.IsHiddenFromPlayer())
                            continue;
                    }

                    // Only proceed if the target is a pawn.
                    if (targetPawn == null)
                        continue;

                    // Check race properties and other flags.
                    if (targetPawn.RaceProps == null || !targetPawn.RaceProps.Humanlike || !targetPawn.Spawned || targetPawn.IsHiddenFromPlayer())
                        continue;

                    // Check if the pawn can be reached.
                    if (!pawn.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly))
                        continue;

                    // Handle same-pawn case separately.
                    if (pawn == targetPawn)
                    {
                        // Actions for self
                        // ...
                    }
                    else
                    {
                        CommonUtil.DebugLog("Creating interaction option");

                        // The previous checks already ensured this is a humanlike pawn.
                        FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionCategory".Translate(targetPawn.Name.ToStringShort), delegate ()
                        {
                            FloatMenuUtility.MakeMenu(GenerateSocialOptions(pawn, target).Where(opt => opt.action != null), (FloatMenuOption opt) => opt.Label, (FloatMenuOption opt) => opt.action);
                        }, MenuOptionPriority.High), pawn, target);
                        opts.Add(option);
                    }
                }
            }
        }

        private static List<FloatMenuOption> GenerateSocialOptions(Pawn pawn, LocalTargetInfo target)
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            FloatMenuOption option = null;
            Pawn targetPawn = target.Pawn;

            if (!pawn.HostileTo(target.Pawn))
            {
                // Social fight
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_SocialFight".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.StartSocialFight(targetPawn);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);

                // Insult
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Insult".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.TryInteractWith(targetPawn, InteractionDefOf.Insult);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);

                // Chat
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Chat".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.TryInteractWith(targetPawn, InteractionDefOf.Chitchat);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);

                // Add option to initiate lovin in bed or not
                // Only add option if pawns are compatible
                // if (LovinUtil.PassesLovinCheck(pawn, target.Pawn, true))
                if (CommonUtil.GetSettings().lovinExtensionEnabled)
                {
                    if (LovinUtil.PassesLovinCheck(pawn, targetPawn, true) && !pawn.Drafted && !targetPawn.Drafted && pawn.TryGetComp<CompExtendedAnimator>() != null)
                    {
                        option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Lovin".Translate(targetPawn.Name.ToStringShort), delegate ()
                        {
                            FloatMenuUtility.MakeMenu(LovinInteractions.GenerateLovinOptions(pawn, targetPawn), (FloatMenuOption opt) => opt.Label, (FloatMenuOption opt) => opt.action);
                        }, MenuOptionPriority.High), pawn, target);
                        option.iconThing = targetPawn;
                        opts.Add(option);
                    }
                    else
                    {
                        /// If lovin' isn't available, tell the user why
                        string text = "Lovin' not available for these pawns";
                        if (pawn.Drafted || targetPawn.Drafted)
                            text = "Lovin' not available when pawns are drafted";

                        string lovinReport = LovinUtil.GetReportForLovin(pawn, targetPawn);
                        text = lovinReport != null ? lovinReport : text;
                        option = new FloatMenuOption(text, delegate () { });
                        FloatMenuUtility.DecoratePrioritizedTask(option, pawn, target);
                        opts.Add(option);
                    }
                }

                // Marriage proposal
                if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Lover, targetPawn) || pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, targetPawn))
                {
                    option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Marriage".Translate(targetPawn.Name.ToStringShort), delegate ()
                    {
                        pawn.interactions.TryInteractWith(targetPawn, InteractionDefOf.MarriageProposal);
                    }, MenuOptionPriority.High), pawn, target);
                    opts.Add(option);
                }

                CompHero heroComp = pawn.TryGetComp<CompHero>();
                if (heroComp != null)
                {
                    List<FloatMenuOption> karmaicAbilityOpts = GetHeroAbilityOptions(pawn, target);
                    
                    if (!karmaicAbilityOpts.NullOrEmpty())
                        opts.AddRange(karmaicAbilityOpts);
                }
            }

            return opts;
        }

        public static List<FloatMenuOption> GetHeroAbilityOptions(Pawn pawn, LocalTargetInfo target)
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            FloatMenuOption option = null;
            Pawn targetPawn = target.Pawn;

            if (KarmaUtil.IsAbilityActive(HeroModule_KarmaicAbilityDefOf.KarmaicAbility_Uplift, pawn))
            {
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Uplift".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.TryInteractWith(target.Pawn, HeroModule_InteractionDefOf.Uplift);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);
            }
            if (KarmaUtil.IsAbilityActive(HeroModule_KarmaicAbilityDefOf.KarmaicAbility_PepTalk, pawn))
            {
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_PepTalk".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.TryInteractWith(target.Pawn, HeroModule_InteractionDefOf.PepTalk);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);
            }
            if (KarmaUtil.IsAbilityActive(HeroModule_KarmaicAbilityDefOf.KarmaicAbility_Proselytize, pawn))
            {
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Proselytize".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.TryInteractWith(target.Pawn, HeroModule_InteractionDefOf.Proselytize);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);
            }
            if (KarmaUtil.IsAbilityActive(HeroModule_KarmaicAbilityDefOf.KarmaicAbility_Oversee, pawn) 
                && target.Pawn.IsSlave 
                && target.Pawn.Faction == pawn.Faction)
            {
                option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InteractionOption_Oversee".Translate(targetPawn.Name.ToStringShort), delegate ()
                {
                    pawn.interactions.TryInteractWith(target.Pawn, HeroModule_InteractionDefOf.Oversee);
                }, MenuOptionPriority.High), pawn, target);
                opts.Add(option);
            }

            return opts;
        }
    }
}
