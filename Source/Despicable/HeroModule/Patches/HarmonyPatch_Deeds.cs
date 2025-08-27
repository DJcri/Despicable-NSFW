using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Despicable
{
    // Handle karma after job completion
    [HarmonyPatch(typeof(Job), "MakeDriver")]
    public class HarmonyPatch_Job
    {
        public static void Postfix(ref JobDriver __result)
        {
            Pawn pawn = __result.pawn;
            CompHero heroComp = pawn.TryGetComp<CompHero>();
            Job job = __result.job;
            LocalTargetInfo target = job.targetA;

            if (target.Pawn != null)
            {
                if (heroComp != null && heroComp.isHero)
                {
                    Dictionary<string, int> deedCounts = heroComp.deedCountValues;
                    __result.AddFinishAction((endCondition) =>
                    {
                        if (endCondition == JobCondition.Succeeded)
                        {
                            switch (job.def.defName)
                            {
                                case "TendPatient":
                                    deedCounts["Treatments"]++;
                                    break;
                                case "Arrest":
                                    if (target.Pawn.HostileTo(pawn))
                                        deedCounts["CriminalArrests"]++;
                                    else
                                        deedCounts["FalseArrests"]++;
                                    break;
                                case "Capture":
                                    if (target.Pawn.HostileTo(pawn))
                                        deedCounts["CriminalArrests"]++;
                                    else
                                        deedCounts["FalseArrests"]++;
                                    break;
                                case "PrisonerEnslave":
                                    deedCounts["EnslavementAttempts"]++;
                                    break;
                                case "GiveToPawn":
                                    deedCounts["Charities"]++;
                                    break;
                            }

                            KarmaUtil.UpdateKarma(heroComp);
                        }
                    });
                }
            }
        }
    }

    // Handle karma memory added
    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[] {typeof(Thought_Memory), typeof(Pawn)})]
    public class HarmonyPatch_Memory
    {
        public static void Postfix(ref Thought_Memory newThought, ref Pawn otherPawn)
        {
            Pawn pawn = newThought?.pawn;

            if (pawn != null)
            {
                CompHero heroComp = pawn.TryGetComp<CompHero>();
                CompHero otherHeroComp = otherPawn?.TryGetComp<CompHero>();
                if (heroComp != null)
                {
                    Dictionary<string, int> deedCounts = heroComp.deedCountValues;
                    Dictionary<string, int> otherPawnDeedCounts = otherHeroComp?.deedCountValues ?? null;

                    switch (newThought.def.defName)
                    {
                        case "RescuedMe":
                            if (otherPawnDeedCounts != null)
                            {
                                otherPawnDeedCounts["Rescues"]++;
                                KarmaUtil.UpdateKarma(otherHeroComp);
                            }
                            break;
                        case "DeniedJoining":
                            deedCounts["PawnsSentAway"]++;
                            break;
                        case "ReleasedHealthyPrisoner":
                            deedCounts["HealthyPrisonersReleased"]++;
                            break;
                        case "ButcheredHumanlikeCorpse":
                            deedCounts["HumansButchered"]++;
                            break;
                        case "Affair":
                            deedCounts["Affairs"]++;
                            break;
                        case "HarmedMe":
                            if (!pawn.HostileTo(otherPawn))
                            {
                                if (!pawn.ageTracker.Adult && otherPawnDeedCounts != null)
                                {
                                    otherPawnDeedCounts["ChildrenHarmed"]++;
                                    otherPawnDeedCounts["InnocentsHarmed"]++;
                                    KarmaUtil.UpdateKarma(otherHeroComp);
                                }
                            }
                            break;
                    }

                    KarmaUtil.UpdateKarma(heroComp);
                }
            }
        }
    }
}
