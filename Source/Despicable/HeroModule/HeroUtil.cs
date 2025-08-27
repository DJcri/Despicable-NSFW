using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public static class HeroUtil
    {
        public static int fearMongerChance = 10;

        public static Pawn FindHero()
        {
            foreach (Pawn pawn in Current.Game.CurrentMap.PlayerPawnsForStoryteller)
            {
                CompHero heroComp = pawn.TryGetComp<CompHero>();
                if (heroComp != null && heroComp.isHero)
                {
                    return pawn;
                }
            }

            return null;
        }
    }
}
