using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace Despicable
{
    public static class Interactions
    {
        public static void OrderedJob(JobDef jobDef, Pawn pawn, LocalTargetInfo target, LovinTypeDef lovinType = null)
        {
            // Set lovin type if applicable
            if (lovinType != null)
                jobDef.GetModExtension<ModExtension_LovinType>().lovinType = lovinType;

            Job job = new Job(jobDef, target);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            pawn.jobs.TryTakeOrderedJob(job);
        }
    }
}
