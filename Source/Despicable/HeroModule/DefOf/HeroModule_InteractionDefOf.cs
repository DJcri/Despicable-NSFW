using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Despicable
{
    [DefOf]
    public class HeroModule_InteractionDefOf
    {
        public static InteractionDef Uplift;
        public static InteractionDef PepTalk;
        public static InteractionDef PetAnimal;
        public static InteractionDef Proselytize;
        public static InteractionDef OperantTraining;
        public static InteractionDef Oversee;

        static HeroModule_InteractionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HeroModule_InteractionDefOf));
        }
    }
}
