using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine.Events;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace Despicable
{
    public class JobDriver_LovinBase : JobDriver
    {
        public readonly TargetIndex iTarget = TargetIndex.A;
        public readonly TargetIndex iBed = TargetIndex.B;
        public readonly TargetIndex iCell = TargetIndex.C;

        public Pawn partnerPawn = null;
        public Building_Bed partnerBed = null;
        public int durationTicks = LovinUtil.defaultDurationTicks;
        List<Pawn> participants = new List<Pawn>();

        public Pawn Partner
        {
            get
            {
                if (partnerPawn != null)
                    return partnerPawn;
                else if (Target is Pawn)
                {
                    LocalTargetInfo localTargetInfo = job.GetTarget(TargetIndex.A).Pawn;
                    partnerBed = localTargetInfo.Pawn.CurrentBed();
                    return job.GetTarget(TargetIndex.A).Pawn;
                }
                else if (Target is Corpse)
                    return (job.GetTarget(TargetIndex.A).Thing as Corpse).InnerPawn;
                else
                    return null;
            }
        }

        public Thing Target
        {
            get
            {
                if (job == null)
                {
                    return null;
                }

                if (job.GetTarget(TargetIndex.A).Pawn != null)
                    return job.GetTarget(TargetIndex.A).Pawn;

                return job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public Building_Bed Bed
        {
            get
            {
                if (partnerBed != null)
                    return partnerBed;
                else if (job.GetTarget(TargetIndex.B).Thing is Building_Bed)
                    return job.GetTarget(TargetIndex.B).Thing as Building_Bed;
                else
                    return null;
            }
        }

        // Lovin toil
        protected Toil LovinToil()
        {
            Toil lovinToil = new Toil();
            lovinToil.defaultCompleteMode = ToilCompleteMode.Never;
            lovinToil.socialMode = RandomSocialMode.Off;
            lovinToil.handlingFacing = true;
            lovinToil.initAction = delegate
            {
                try
                {
                    // Face each other, in case there's no animation
                    pawn.rotationTracker.FaceTarget(Partner);
                    Partner.rotationTracker.FaceTarget(pawn);

                    // Stop partner's pathing
                    Partner.pather.StopDead();
                    Partner.jobs.curDriver.asleep = false;

                    // ANIMATION
                    // Find animation group that fits the context
                    participants.Clear();
                    participants.AddDistinct(pawn);
                    participants.AddDistinct(Partner);
                    durationTicks = LovinUtil.defaultDurationTicks;
                    if (CommonUtil.GetSettings().animationExtensionEnabled)
                    {
                        List<AnimGroupDef> playableAnimations = ContextUtil.GetPlayableAnimationsFor(participants, job.def.GetModExtension<ModExtension_LovinType>()?.lovinType);

                        if (!playableAnimations.NullOrEmpty())
                        {
                            AnimGroupDef animGroupDef = playableAnimations.RandomElement();
                            Dictionary<string, Pawn> roleAssignments = ContextUtil.AssignRoles(animGroupDef, participants);
                            Thing anchor = (partnerBed != null) ? (Thing)Bed : pawn;
                            // If no bed, play animation on initiator pawn's position
                            if (roleAssignments != null)
                            {
                                AnimUtil.PlayAnimationGroup(animGroupDef, roleAssignments, anchor);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    CommonUtil.DebugLog(e.ToString());
                }
            };
            lovinToil.AddPreTickAction(delegate
            {
                // Tick logic here
                durationTicks--;

                // Try to play handle lovin animation
                // Otherwise use duration ticks
                try
                {
                    foreach (Pawn participant in participants)
                    {
                        if (pawn.IsHashIntervalTick(LovinUtil.restDepletionInterval))
                            participant.needs.rest?.NeedInterval();
                        if (pawn.IsHashIntervalTick(LovinUtil.ticksBetweenHearts))
                            FleckMaker.ThrowMetaIcon(participant.Position, participant.Map, FleckDefOf.Heart);
                    }

                    // End once animations finish or reaches default duration
                    CompExtendedAnimator animator = pawn.TryGetComp<CompExtendedAnimator>();
                    if ((!(animator.animQueue.Count > 0) && !animator.hasAnimPlaying)
                    || (!(animator.hasAnimPlaying) && durationTicks <= 0))
                    {
                        ReadyForNextToil();
                    }
                    else if (durationTicks <= 0)
                    {
                        ReadyForNextToil();
                    }
                }
                catch (Exception e)
                {
                    CommonUtil.DebugLog(e.ToString());
                }

                if (durationTicks <= 0)
                    ReadyForNextToil();
            });
            lovinToil.AddFinishAction(delegate
            {
                // Restart partner's pathing and animation
                Partner.pather.StartPath(Target, PathEndMode.OnCell);
                AnimUtil.ResetAnimatorsForGroup(participants);
            });
            lovinToil.FailOn(() => !PartnerPresent(Partner));

            return lovinToil;
        }

        // Gives memory of lovin' to pawns
        protected Toil FinalizeLovinToil()
        {
            Toil FinalizeLovinToil = new Toil()
            {
                initAction = () =>
                {
                    LovinTypeDef lovinType = pawn.CurJob.def.GetModExtension<ModExtension_LovinType>()?.lovinType;
                    InteractionDef interactionDef = null;

                    if (lovinType != null)
                        interactionDef = lovinType.interaction;
                    if (interactionDef != null)
                        pawn.interactions.TryInteractWith(Partner, interactionDef);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            return FinalizeLovinToil;
        }

        // Check if partner pawn is still valid
        protected void PreInit()
        {
            pawn.jobs.curJob.playerForced = false;
            this.FailOnDespawnedNullOrForbidden(iTarget);
            this.FailOn(() => !Spawned(Partner));
            this.FailOn(() => !LovinUtil.PassesLovinCheck(pawn, Partner, true));
            this.FailOn(() => !NotDrafted(Partner));
        }

        // Predicate conditions where lovin' should end if not met
        protected bool Spawned(Pawn partner) =>
            partner.Spawned && partner.Map == pawn.Map;
        protected bool NotDrafted(Pawn partner) =>
            !partner.Drafted && !pawn.Drafted;
        protected bool PartnerPresent(Pawn partner) =>
            (partner.jobs.curDriver as JobDriver_LovinBase)?.Partner == pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            return null;
        }
    }
}
