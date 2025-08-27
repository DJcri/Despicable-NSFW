using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class CompProperties_Hero : CompProperties
    {
        public CompProperties_Hero()
        {
            base.compClass = typeof(CompHero);
        }
    }
}
