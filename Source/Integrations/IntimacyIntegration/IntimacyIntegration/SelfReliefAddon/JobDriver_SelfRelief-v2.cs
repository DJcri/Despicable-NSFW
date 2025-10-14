using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using UnityEngine;
using LoveyDoveySexWithEuterpe;
using Despicable;
using System;

namespace SelfReliefAddon
{
    public class JobDriver_SelfRelief : JobDriver
    {
        private const int BaseDuration = 2500; // Похоже на время Lovin

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Найти подходящее место (спальня или уединенное место)
            yield return FindBestPrivateSpot();
            
            // Идти к месту
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            
            // Выполнить самоудовлетворение (имитация Lovin)
            yield return PerformSelfReliefLikeLovin();
        }

        private Toil FindBestPrivateSpot()
        {
            return new Toil
            {
                initAction = delegate
                {
                    IntVec3 spot = FindPrivateSpot(pawn);
                    if (spot.IsValid)
                    {
                        job.SetTarget(TargetIndex.A, spot);
                    }
                    else
                    {
                        // Если не нашли приватное место, используем текущую позицию
                        job.SetTarget(TargetIndex.A, pawn.Position);
                    }
                }
            };
        }

        private Toil PerformSelfReliefLikeLovin()
        {
            return new Toil
            {
                initAction = delegate
                {
                    job.locomotionUrgency = LocomotionUrgency.None;

                    // Animation handling
                    try
                    {
                        int durationTicks = LovinUtil.defaultDurationTicks;
                        if (CommonUtil.GetSettings().animationExtensionEnabled)
                        {
                            List<Pawn> participants = new List<Pawn> { pawn }; /// Only pawn's self :C
                            // Finds an "animation group" to play that fits the context of having only one participant
                            LovinTypeDef soloLovinType = DefDatabase<LovinTypeDef>.GetNamedSilentFail("Solo");
                            List<AnimGroupDef> playableAnimations = ContextUtil.GetPlayableAnimationsFor(participants, soloLovinType);

                            if (!playableAnimations.NullOrEmpty())
                            {
                                AnimGroupDef animGroupDef = playableAnimations.RandomElement();
                                Dictionary<string, Pawn> roleAssignments = ContextUtil.AssignRoles(animGroupDef, participants);
                                // Could set a bed as anchor, but since this driver doesn't inherit from LovinBase, we just use the pawn
                                // Since there's no Bed property as of now, like in LovinBase
                                Thing anchor = pawn;
                                if (roleAssignments != null)
                                {
                                    AnimUtil.PlayAnimationGroup(animGroupDef, roleAssignments, anchor);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                },
                tickAction = delegate
                {
                    // Имитируем поведение во время Lovin
                    if (ticksLeftThisToil % 150 == 0) // Каждые ~2.5 секунды
                    {
                        // Добавляем небольшие визуальные эффекты
                        if (Rand.Chance(0.3f))
                        {
                            FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
                        }
                    }
                },
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = BaseDuration,
                handlingFacing = true,
                finishActions = new List<System.Action>
                {
                    delegate
                    {
                        ApplySelfReliefEffects(pawn);
                    }
                }
            };
        }

        private IntVec3 FindPrivateSpot(Pawn pawn)
        {
            // Пытаемся найти спальню пешки (первый приоритет)
            Room bedroom = pawn.ownership?.OwnedRoom;
            if (bedroom != null && !bedroom.PsychologicallyOutdoors)
            {
                IntVec3 bedSpot = bedroom.Cells.RandomElement();
                if (bedSpot.Standable(pawn.Map) && bedSpot.GetDoor(pawn.Map) == null)
                {
                    return bedSpot;
                }
            }

            // Ищем кровать пешки (второй приоритет)
            Building_Bed bed = pawn.ownership?.OwnedBed;
            if (bed != null)
            {
                IntVec3 bedArea = bed.Position;
                for (int i = 0; i < 10; i++)
                {
                    IntVec3 candidate = bedArea + GenRadial.RadialPattern[i];
                    if (candidate.InBounds(pawn.Map) && candidate.Standable(pawn.Map))
                    {
                        return candidate;
                    }
                }
            }

            // Ищем любую закрытую комнату (третий приоритет)
            var rooms = pawn.Map.regionGrid.AllRooms
                .Where(r => !r.PsychologicallyOutdoors && 
                           !r.IsHuge && 
                           r.Role.defName != "DiningRoom" &&
                           r.Role.defName != "RecRoom");

            foreach (Room room in rooms)
            {
                IntVec3 roomSpot = room.Cells.RandomElement();
                if (roomSpot.Standable(pawn.Map) && 
                    !SelfReliefUtility.HasNearbyColonists(pawn, roomSpot, 5))
                {
                    return roomSpot;
                }
            }

            return IntVec3.Invalid;
        }

        private void ApplySelfReliefEffects(Pawn pawn)
        {
            // 1. Добавляем статус самоудовлетворения
            Hediff selfReliefStatus = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamed("SelfRelief_Status"));
            
            if (selfReliefStatus != null)
            {
                selfReliefStatus.Severity = 1.0f;
            }
            else
            {
                Hediff newStatus = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamed("SelfRelief_Status"), pawn);
                newStatus.Severity = 1.0f;
                pawn.health.AddHediff(newStatus);
            }

            // 2. Влияем на потребность Lovin (но не убираем полностью)
            ModifyLovinNeed(pawn);

            // 3. Добавляем модификатор потребности Lovin
            Hediff lovinModifier = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamed("SelfRelief_LovinNeedModifier"));
            
            if (lovinModifier != null)
            {
                lovinModifier.Severity = 1.0f;
            }
            else
            {
                Hediff newModifier = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamed("SelfRelief_LovinNeedModifier"), pawn);
                newModifier.Severity = 1.0f;
                pawn.health.AddHediff(newModifier);
            }

            // 4. Небольшое восстановление отдыха (как при Lovin)
            Need_Rest rest = pawn.needs.rest;
            if (rest != null)
            {
                rest.CurLevel += 0.05f; // Меньше чем при обычном Lovin
            }

            // 5. Интеграция с другими модами
            SelfReliefUtility.TryIntegrateWithOtherMods(pawn);

            // 6. Логируем для отладки
            SelfReliefUtility.LogSelfReliefAction(pawn, "completed self-relief session");
        }

        private void ModifyLovinNeed(Pawn pawn)
        {
            var lovinNeed = pawn.needs.TryGetNeed<Need_Intimacy>();
            if (lovinNeed == null) return;

            // Увеличиваем потребность Lovin, но не позволяем ей подняться выше 60%
            float currentLevel = lovinNeed.CurLevel;
            float maxIncrease = 0.6f - currentLevel; // Максимум до 60%
            
            if (maxIncrease > 0)
            {
                float actualIncrease = Mathf.Min(0.4f, maxIncrease); // Увеличиваем на 40% или до максимума
                lovinNeed.CurLevel += actualIncrease;
                
                SelfReliefUtility.LogSelfReliefAction(pawn, 
                    $"Lovin need increased by {actualIncrease:F2}, new level: {lovinNeed.CurLevel:F2}");
            }
        }
    }
}