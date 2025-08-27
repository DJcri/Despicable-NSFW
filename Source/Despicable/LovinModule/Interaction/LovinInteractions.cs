using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class LovinInteractions
    {
        public static List<FloatMenuOption> GenerateLovinOptions(Pawn pawn, LocalTargetInfo target)
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            FloatMenuOption option = null;
            
            if (!pawn.HostileTo(target.Pawn))
            {
                foreach (LovinTypeDef lovinTypeDef in DefDatabase<LovinTypeDef>.AllDefsListForReading.ToList())
                {
                    // Check lovin type gender requirements
                    if (lovinTypeDef.requiresFemale)
                    {
                        if (pawn.gender != Gender.Female && target.Pawn.gender != Gender.Female)
                        {
                            continue;
                        }
                    }
                    if (lovinTypeDef.requiresMale)
                    {
                        if (pawn.gender != Gender.Male && target.Pawn.gender != Gender.Male)
                        {
                            continue;
                        }
                    }
                    option = new FloatMenuOption(lovinTypeDef.defName, delegate ()
                    {
                        JobDef job = null;
                        if (target.Pawn.InBed())
                            job = LovinModule_JobDefOf.Job_GetBedLovin;
                        else
                            job = LovinModule_JobDefOf.Job_GetLovin;
                        Interactions.OrderedJob(job, pawn, target, lovinTypeDef);
                    }, pawn.def, lovinTypeDef.interaction.GetSymbol());
                    FloatMenuUtility.DecoratePrioritizedTask(option, pawn, target);
                    opts.Add(option);
                }
            }

            return opts;
        }
    }
}
