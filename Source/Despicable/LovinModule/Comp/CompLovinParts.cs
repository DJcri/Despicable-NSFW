using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Despicable
{
    /// <summary>
    /// Handles RENDERING of GENITALIA / NUDITY
    /// Can be TOGGLED ON/OFF
    /// </summary>
    public class CompLovinParts : ThingComp
    {
        public static bool enabled = CommonUtil.GetSettings().nudityEnabled;
        private int ticks = 0;
        private int ticksBetweenUpdates = 2400;

        public override void CompTick()
        {
            // Sync settings in intervals for performance
            ticks++;
            if (ticks % ticksBetweenUpdates == 0)
            {
                enabled = CommonUtil.GetSettings().nudityEnabled;
            }

            base.CompTick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        private Pawn pawn => base.parent as Pawn;

        public override List<PawnRenderNode> CompRenderNodes()
        {
            List<PawnRenderNode> lovinPartNodes = new List<PawnRenderNode>();
            if (!pawn.RaceProps.Humanlike || !enabled || !pawn.ageTracker.Adult || !(PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Male))) //** <-- TEMPORARY, REMOVE IF GENITALIA FOR OPPOSITE SEX IS IMPLEMENTED
                return null;

            // Assign genitalia
            GenitalDef genitalDef = LovinModule_GenitalDefOf.Genital_Penis;
            PawnRenderNode genitalNode;

            bool hasPants = false;
            bool hasShirt = false;
            pawn.apparel?.HasBasicApparel(out hasPants, out hasShirt);

            if ((genitalDef != null && !hasPants)
                || pawn.TryGetComp<CompExtendedAnimator>().hasAnimPlaying)
            {
                PawnRenderNodeProperties genitalProps = DefDatabase<GenitalDef>.GetNamed(genitalDef.defName).properties;
                genitalProps.anchorTag = "Body";
                if (pawn.TryGetComp<CompExtendedAnimator>().hasAnimPlaying)
                {
                    genitalProps.texPath = genitalDef.texPathAroused; /// Use aroused texture in the right context
                }
                else
                {
                    genitalProps.texPath = genitalDef.properties.texPath; /// Default texture path (flaccid / unaroused)
                }

                genitalNode = CommonUtil.CreateNode(pawn, genitalProps, PawnRenderNodeTagDefOf.Body);
                lovinPartNodes.Add(genitalNode);
            }

            return lovinPartNodes;
        }
    }
}