using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Despicable;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class AnimGroupDef : Def
    {
        public List<AnimRoleDef> animRoles;
        public int numActors = 1;
        public List<int> loopIndex;
        public List<LovinTypeDef> lovinTypes;
    }
}