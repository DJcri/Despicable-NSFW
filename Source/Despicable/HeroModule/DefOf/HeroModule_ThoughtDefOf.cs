using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Despicable
{
    [DefOf]
    public class HeroModule_ThoughtDefOf
    {
        public static ThoughtDef PepTalkReceiver;
        public static ThoughtDef OverseeReceiver;

        static HeroModule_ThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HeroModule_ThoughtDefOf));
        }
    }
}
