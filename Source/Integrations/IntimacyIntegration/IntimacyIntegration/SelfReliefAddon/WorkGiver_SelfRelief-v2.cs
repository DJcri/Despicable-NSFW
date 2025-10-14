using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using LoveyDoveySexWithEuterpe;

namespace SelfReliefAddon
{
    public class WorkGiver_SelfRelief : WorkGiver
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return false; // Не используем конкретные объекты
        }

        public override Job NonScanJob(Pawn pawn)
        {
            // Проверяем, нужно ли пешке самоудовлетворение
            if (!ShouldSeekSelfRelief(pawn))
            {
                return null;
            }

            // Создаем новое задание
            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("SelfRelief"));
            return job;
        }

        private bool ShouldSeekSelfRelief(Pawn pawn)
        {
            // Базовые проверки
            if (!pawn.IsColonist || pawn.Dead || pawn.Downed)
                return false;

            // Проверяем возраст (только взрослые)
            if (!pawn.ageTracker.Adult)
                return false;

            // Проверяем способности
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness) ||
                !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                return false;

            // Не делаем, если пешка призвана
            if (pawn.Drafted)
                return false;

            // Не делаем, если есть недавний статус самоудовлетворения
            if (pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("SelfRelief_Status")))
                return false;

            // Проверяем черты характера (аскеты не нуждаются)
            if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
                return false;

            // Проверяем потребность в lovin - основная логика
            Need_Intimacy lovinNeed = pawn.needs.TryGetNeed<Need_Intimacy>();
            if (lovinNeed == null)
                return false;

            // Проверяем основные триггеры согласно комментарию:
            return ShouldTriggerSelfRelief(pawn, lovinNeed);
        }

        private bool ShouldTriggerSelfRelief(Pawn pawn, Need_Intimacy lovinNeed)
        {
            // Триггер 1: Нет lovin в течение длительного времени
            if (HasBeenTooLongWithoutLovin(pawn, lovinNeed))
                return true;

            // Триггер 2: Нет партнера в любых отношениях
            if (!HasAnyRomanticPartner(pawn))
                return true;

            // Триггер 3: Не может найти подходящего партнера на карте
            if (!CanFindSuitablePartnerOnMap(pawn))
                return true;

            return false;
        }

        private bool HasBeenTooLongWithoutLovin(Pawn pawn, Need_Intimacy lovinNeed)
        {
            // Если потребность в lovin очень низкая (ниже 30%) и есть негативные мысли
            if (lovinNeed.CurLevel < 0.3f)
            {
                // Проверяем, есть ли мысли о недостатке lovin
                var thoughts = pawn.needs.mood.thoughts.memories.Memories;
                bool hasNoLovinThoughts = thoughts.Any(t => 
                    t.def.defName.Contains("LovinTake") && t.CurStage.baseMoodEffect < 0);
                
                return hasNoLovinThoughts;
            }
            return false;
        }

        private bool HasAnyRomanticPartner(Pawn pawn)
        {
            // Проверяем наличие любого романтического партнера
            return pawn.relations.GetDirectRelationsCount(PawnRelationDefOf.Spouse) > 0 ||
                   pawn.relations.GetDirectRelationsCount(PawnRelationDefOf.Lover) > 0 ||
                   pawn.relations.GetDirectRelationsCount(PawnRelationDefOf.Fiance) > 0;
        }

        private bool CanFindSuitablePartnerOnMap(Pawn pawn)
        {
            // Проверяем наличие потенциальных партнеров на карте
            var potentialPartners = pawn.Map.mapPawns.FreeColonists
                .Where(p => p != pawn && 
                           p.ageTracker.Adult && 
                           !p.Dead && 
                           !p.Downed &&
                           SelfReliefUtility.AreCompatibleForRomance(pawn, p));

            return potentialPartners.Any();
        }

        public override float GetPriority(Pawn pawn, TargetInfo target)
        {
            // Приоритет зависит от уровня потребности в lovin
            Need_Intimacy lovinNeed = pawn.needs.TryGetNeed<Need_Intimacy>();
            if (lovinNeed == null) return 0f;

            float lovinLevel = lovinNeed.CurLevel;
            
            // Чем ниже потребность в lovin, тем выше приоритет
            if (lovinLevel < 0.15f) return 9.0f; // Критически низкий уровень
            if (lovinLevel < 0.30f) return 7.5f; // Очень низкий
            if (lovinLevel < 0.50f) return 6.0f; // Низкий
            if (lovinLevel < 0.70f) return 4.0f; // Ниже среднего
            
            return 2.0f; // Низкий базовый приоритет
        }
    }
}