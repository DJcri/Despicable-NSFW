using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Despicable; 
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace Despicable
{
    public class JobDriver_LovinNoBed : JobDriver_LovinBase
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Target, job, LovinUtil.maxLovinPartners, 0, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            PreInit();

            JobDef PartnerJobDef = LovinModule_JobDefOf.Job_GiveLovin;

            // Vanilla Toils
            yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

            // Custom Toils
            Toil WaitForPartner = new Toil();
            WaitForPartner.defaultCompleteMode = ToilCompleteMode.Delay;
            WaitForPartner.initAction = delegate
            {
                ticksLeftThisToil = 5000;
            };
            WaitForPartner.tickAction = delegate
            {
                pawn.GainComfortFromCellIfPossible(durationTicks - 1);
                if (pawn.Position.DistanceTo(Partner.Position) <= 1f)
                {
                    ReadyForNextToil();
                }
            };
            yield return WaitForPartner;

            Toil StartPartnerJob = new Toil();
            StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
            StartPartnerJob.socialMode = RandomSocialMode.Off;
            StartPartnerJob.initAction = delegate
            {
                Job partnerJob = JobMaker.MakeJob(PartnerJobDef, pawn);
                Partner.jobs.StartJob(partnerJob, JobCondition.InterruptForced);
            };

            yield return StartPartnerJob;
            yield return LovinToil();

            // Finalize lovin
            yield return FinalizeLovinToil();
        }
    }
}
