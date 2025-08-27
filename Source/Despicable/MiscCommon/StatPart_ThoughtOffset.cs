using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class StatPart_ThoughtOffset : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Pawn pawn && ActiveFor(pawn))
            {
                return "StatsReport_ThoughtOffset".Translate() + ": " + ThoughtOffset(pawn).ToString("0.00") + "%";
            }
            return "";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req == null) return;
            if (req.Pawn == null) return;

            val += ThoughtOffset(req.Pawn);
        }

        public float ThoughtOffset(Pawn pawn)
        {
            float offset = 0f;

            StatDef stat = parentStat;

            if (stat != null)
            {
                switch (stat.defName)
                {
                    // Set the maximum buff offset the player hero can receive
                    case "WorkSpeedGlobal":
                        if (pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(HeroModule_ThoughtDefOf.PepTalkReceiver) != null)
                            offset += 10f;
                        if (pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(HeroModule_ThoughtDefOf.OverseeReceiver) != null)
                            offset += 10f;
                        break;
                }
            }

            return offset;
        }

        private bool ActiveFor(Pawn pawn)
        {
            return true;
        }
    }
}
