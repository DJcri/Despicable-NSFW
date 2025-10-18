using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class CustomLovinInitCheck
    {
        // The same as Intimacy's but doesn't check for if sleeping, in case the user is ordering lovin'
        public static bool CanInitiateInteraction(Pawn pawn, InteractionDef interactionDef = null)
        {
            if (pawn.interactions == null)
            {
                return false;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return false;
            }

            /*
            if (!pawn.Awake())
            {
                return false;
            }
            */

            if (pawn.IsBurning())
            {
                return false;
            }

            if (pawn.IsMutant && pawn.mutant.Def.incapableOfSocialInteractions)
            {
                return false;
            }

            if (pawn.IsInteractionBlocked(interactionDef, isInitiator: true, isRandom: false))
            {
                return false;
            }

            return true;
        }
    }
}
