using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Despicable
{
    public static class PawnStateUtil
    {
        public static bool IsAsleep(Pawn pawn)
        {
            if (pawn.InBed() && !LovinUtil.IsLovin(pawn))
                return true;
            return false;
        }

        public static bool IsDrunk(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.AlcoholHigh))
                return true;
            return false;
        }

        public static bool IsTired(Pawn pawn)
        {
            if (pawn.needs?.rest?.CurLevelPercentage <= 0.3f)
                return true;
            return false;
        }

        public static bool hasMentalBreak(Pawn pawn)
        {
            if (pawn.InMentalState)
                return true;
            return false;
        }

        public static bool isBerserk(Pawn pawn)
        {
            if (pawn.InAggroMentalState)
                return true;
            return false;
        }

        public static bool isInfant(Pawn pawn)
        {
            int curLifeStage = pawn.ageTracker.CurLifeStageIndex;
            if (curLifeStage == 0 || curLifeStage == 1)
                return true;
            return false;
        }

        public static bool ComparePawnGenderToByte(Pawn pawn, byte otherGender)
        {
            byte pawnGender = (byte)pawn.gender;
            if (pawnGender == otherGender) return true;
            return false;
        }
    }
}
