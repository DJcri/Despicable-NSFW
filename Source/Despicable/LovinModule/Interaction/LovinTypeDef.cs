using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class LovinTypeDef : Def
    {
        public InteractionDef interaction;
        public bool requiresMale = false;
        public bool requiresFemale = false;
    }
}
