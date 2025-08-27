using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace Despicable
{
    [DefOf]
    public class HeroModule_KarmaicAbilityDefOf
    {
        public static KarmaicAbilityDef KarmaicAbility_Uplift;
        public static KarmaicAbilityDef KarmaicAbility_PepTalk;
        public static KarmaicAbilityDef KarmaicAbility_PetAnimal;
        public static KarmaicAbilityDef KarmaicAbility_Proselytize;
        public static KarmaicAbilityDef KarmaicAbility_Torture;
        public static KarmaicAbilityDef KarmaicAbility_OperantTraining;
        public static KarmaicAbilityDef KarmaicAbility_Oversee;
        public static KarmaicAbilityDef KarmaicAbility_FearMonger;

        static HeroModule_KarmaicAbilityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HeroModule_KarmaicAbilityDefOf));
        }
    }
}
