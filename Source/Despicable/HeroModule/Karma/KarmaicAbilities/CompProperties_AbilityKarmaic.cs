using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Despicable
{
    public class CompProperties_AbilityKarmaic : CompProperties_AbilityEffect
    {
        public KarmaicAbilityDef karmaicAbilityDef;

        public CompProperties_AbilityKarmaic()
        {
            compClass = typeof(CompAbilityEffect_Karmaic);
        }
    }
}
