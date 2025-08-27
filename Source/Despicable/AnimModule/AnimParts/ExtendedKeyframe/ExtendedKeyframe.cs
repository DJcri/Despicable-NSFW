using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class ExtendedKeyframe : Keyframe
    {
        public int? variant;
        public Rot4 rotation = Rot4.North;
        public SoundDef sound = null;
        public bool visible = false;
        public FacialAnimDef facialAnim = null;
    }
}