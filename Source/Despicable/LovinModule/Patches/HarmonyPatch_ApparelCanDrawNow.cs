using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body), "CanDrawNow")]
    public static class HarmonyPatch_ApparelBody_CanDrawNow
    {
        [HarmonyPostfix]
        public static void Postfix(PawnRenderNode __instance, PawnDrawParms parms, ref bool __result)
        {
            Pawn pawn = parms.pawn;
            if (LovinUtil.IsLovin(pawn))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "CanDrawNow")]
    public static class HarmonyPatch_ApparelHead_CanDrawNow
    {
        [HarmonyPostfix]
        public static void Postfix(PawnRenderNode __instance, PawnDrawParms parms, ref bool __result)
        {
            Pawn pawn = parms.pawn;
            if (LovinUtil.IsLovin(pawn))
            {
                __result = false;
            }
        }
    }

    // Prevent pawns from becoming bald while 'lovin'.
    [HarmonyPatch(typeof(PawnRenderTree), "ParallelPreDraw")]
    public static class HarmonyPatch_PawnRenderTreeBaldingFix
    {
        // Use a Prefix to modify the parms before they're used for rendering.
        public static void Prefix(ref PawnDrawParms parms, PawnRenderTree __instance)
        {
            // We can now access the pawn from the PawnRenderTree instance.
            Pawn pawn = __instance.pawn;

            if (pawn != null && LovinUtil.IsLovin(pawn))
            {
                // Unset the Headgear and Clothes flags.
                parms.flags &= ~PawnRenderFlags.Headgear;
                parms.flags &= ~PawnRenderFlags.Clothes;
            }
        }
    }
}
