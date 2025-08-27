using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Body), "CanDrawNow")]
    public class HarmonyPatch_PawnRenderNodeWorkerBody
    {
        // Allows body to render if bed lovin occurs
        public static void Postfix(ref bool __result, ref PawnRenderNode node, ref PawnDrawParms parms)
        {
            Pawn pawn = parms.pawn;
            if (LovinUtil.IsLovin(pawn))
            {
                __result = true;
            }
        }
    }
}
