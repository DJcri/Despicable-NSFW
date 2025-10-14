using System;
using System.Reflection;
using System.Linq;
using Verse;
using RimWorld;

namespace SelfReliefAddon
{
    public static class SelfReliefUtility
    {
        // Кэшируем информацию о модах для производительности
        private static bool? intimacyModLoaded = null;
        private static bool? despicableModLoaded = null;

        public static bool IsIntimacyModLoaded()
        {
            if (intimacyModLoaded == null)
            {
                intimacyModLoaded = ModLister.AllInstalledMods.Any(mod => 
                    mod.Name.Contains("Intimacy") || mod.PackageId.Contains("intimacy"));
            }
            return intimacyModLoaded.Value;
        }

        public static bool IsDespicableModLoaded()
        {
            if (despicableModLoaded == null)
            {
                despicableModLoaded = ModLister.AllInstalledMods.Any(mod => 
                    mod.Name.Contains("Despicable") || mod.PackageId.Contains("despicable"));
            }
            return despicableModLoaded.Value;
        }

        public static void TryIntegrateWithOtherMods(Pawn pawn)
        {
            TryIntegrateWithIntimacyMod(pawn);
            TryIntegrateWithDespicableMod(pawn);
        }

        public static void TryIntegrateWithIntimacyMod(Pawn pawn)
        {
            if (!IsIntimacyModLoaded()) return;

            try
            {
                var intimacyAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Contains("Intimacy"));
                
                if (intimacyAssembly != null)
                {
                    var intimacyManagerType = intimacyAssembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Contains("IntimacyManager") || t.Name.Contains("IntimacyHandler"));
                    
                    if (intimacyManagerType != null)
                    {
                        var addSatisfactionMethod = intimacyManagerType.GetMethod("AddSelfSatisfaction", 
                            BindingFlags.Public | BindingFlags.Static);
                        
                        if (addSatisfactionMethod != null)
                        {
                            addSatisfactionMethod.Invoke(null, new object[] { pawn, 0.25f }); // 25% удовлетворения
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"SelfRelief: Failed to integrate with Intimacy mod: {ex.Message}");
            }
        }

        public static void TryIntegrateWithDespicableMod(Pawn pawn)
        {
            if (!IsDespicableModLoaded()) return;

            try
            {
                var despicableAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Contains("Despicable"));
                
                if (despicableAssembly != null)
                {
                    var karmaManagerType = despicableAssembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Contains("Karma"));
                    
                    if (karmaManagerType != null)
                    {
                        var addKarmaMethod = karmaManagerType.GetMethod("AddNeutralAction", 
                            BindingFlags.Public | BindingFlags.Static);
                        
                        if (addKarmaMethod != null)
                        {
                            addKarmaMethod.Invoke(null, new object[] { pawn, "SelfRelief" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"SelfRelief: Failed to integrate with Despicable mod: {ex.Message}");
            }
        }

        public static bool AreCompatibleForRomance(Pawn pawn1, Pawn pawn2)
        {
            // Проверка романтической совместимости между двумя пешками
            if (pawn1 == pawn2) return false;
            if (!pawn1.ageTracker.Adult || !pawn2.ageTracker.Adult) return false;

            // Проверяем сексуальную ориентацию
            float romanceChance = pawn1.relations.SecondaryRomanceChanceFactor(pawn2);
            if (romanceChance <= 0f) return false;

            // Проверяем мнение друг о друге
            int opinion1 = pawn1.relations.OpinionOf(pawn2);
            int opinion2 = pawn2.relations.OpinionOf(pawn1);
            
            // Базовое требование - не враждебное отношение
            if (opinion1 < -20 || opinion2 < -20) return false;

            // Проверяем существующие отношения
            if (pawn1.relations.DirectRelationExists(PawnRelationDefOf.Spouse, pawn2) ||
                pawn1.relations.DirectRelationExists(PawnRelationDefOf.Lover, pawn2) ||
                pawn1.relations.DirectRelationExists(PawnRelationDefOf.ExLover, pawn2))
            {
                return true; // Уже партнеры
            }

            // Проверяем семейные связи (исключаем родственников)
            if (pawn1.relations.FamilyByBlood.Contains(pawn2)) return false;

            return true;
        }

        public static bool HasNearbyColonists(Pawn centerPawn, IntVec3 position, int radius)
        {
            if (centerPawn?.Map == null) return false;

            var nearbyPawns = GenRadial.RadialDistinctThingsAround(
                position, centerPawn.Map, radius, false)
                .OfType<Pawn>()
                .Where(p => p != centerPawn && 
                           p.IsColonist && 
                           p.Awake() && 
                           !p.Dead && 
                           !p.Downed);

            return nearbyPawns.Any();
        }

        public static bool HasNearbyColonists(Pawn centerPawn, int radius)
        {
            return HasNearbyColonists(centerPawn, centerPawn.Position, radius);
        }

        public static bool IsInPublicArea(Pawn pawn)
        {
            Room room = pawn.GetRoom();
            if (room == null) return true; // Снаружи = публично

            // Публичные помещения
            if (room.Role == RoomRoleDefOf.DiningRoom ||
                room.Role == RoomRoleDefOf.RecRoom ||
                room.Role == DefDatabase<RoomRoleDef>.GetNamedSilentFail("Workshop") ||
                room.Role == DefDatabase<RoomRoleDef>.GetNamedSilentFail("Kitchen"))
            {
                return true;
            }

            // Большие комнаты обычно публичные
            if (room.CellCount > 50) return true;

            return false;
        }

        public static bool CanPawnPerformSelfRelief(Pawn pawn)
        {
            // Базовые проверки
            if (pawn?.Map == null || pawn.Dead || pawn.Downed) return false;
            if (!pawn.ageTracker.Adult) return false;
            if (!pawn.IsColonist) return false;

            // Не можем, если пешка в состоянии ментального срыва
            if (pawn.MentalStateDef != null) return false;

            // Не можем, если пешка в бою
            if (pawn.InCombat) return false;

            // Проверяем настройки мода
            if (!SelfReliefSettings.enableSelfRelief) return false;

            // Проверяем требование приватности
            if (SelfReliefSettings.requirePrivacy)
            {
                if (HasNearbyColonists(pawn, 5)) return false;
                if (IsInPublicArea(pawn)) return false;
            }

            // Проверяем черты характера
            if (SelfReliefSettings.respectTraits && 
                pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                return false;
            }

            return true;
        }

        public static void LogSelfReliefAction(Pawn pawn, string action)
        {
            if (SelfReliefSettings.debugMode || Prefs.DevMode)
            {
                Log.Message($"SelfRelief: {pawn.LabelShort} - {action}");
            }
        }

        // Вспомогательная функция для получения времени с последнего Lovin
        public static int GetTicksSinceLastLovin(Pawn pawn)
        {
            var memories = pawn.needs.mood.thoughts.memories.Memories;
            var lastLovinThought = memories
                .Where(t => t.def.defName.Contains("GotSomeLovin"))
                .OrderByDescending(t => t.age)
                .FirstOrDefault();

            if (lastLovinThought != null)
            {
                return lastLovinThought.age;
            }

            // Если нет мыслей о lovin, возвращаем большое число
            return int.MaxValue;
        }

        // Проверка, есть ли у пешки активный хедифф модификации потребности Lovin
        public static bool HasLovinNeedModifier(Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(
                DefDatabase<HediffDef>.GetNamed("SelfRelief_LovinNeedModifier"));
        }

        // Получение текущего уровня потребности Lovin с учетом модификации
        public static float GetEffectiveLovinNeedLevel(Pawn pawn)
        {
            Need_Lovin lovinNeed = pawn.needs.TryGetNeed<Need_Lovin>();
            if (lovinNeed == null) return 1f;

            float baseLevel = lovinNeed.CurLevel;
            
            // Если есть модификатор самоудовлетворения, учитываем его
            if (HasLovinNeedModifier(pawn))
            {
                // Модификатор эффективно "поднимает" ощущение потребности в пределах 60%
                return Mathf.Min(baseLevel + 0.2f, 0.6f);
            }

            return baseLevel;
        }
    }
}