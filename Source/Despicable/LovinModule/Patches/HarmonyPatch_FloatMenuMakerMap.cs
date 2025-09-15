using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Despicable;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    static class HarmonyPatch_FloatMenuMakerMap
    {
        public static void Postfix(List<FloatMenuOption> __result, List<Pawn> selectedPawns, Vector3 clickPos)
        {
            // Safety check for the result list itself, although it's unlikely to be null.
            if (__result == null) return;

            // Your original null/empty check is correct.
            if (selectedPawns.NullOrEmpty()) return;

            // Safety check for the specific pawn.
            Pawn pawn = selectedPawns[0];
            if (pawn == null) return;

            // Call your method with the now-safely-checked objects.
            try
            {
                InteractionMenu.InitInteractionMenu(pawn, __result, clickPos);
            }
            catch (Exception e)
            {
                Log.Error($"[Despicable] Error in HarmonyPatch_FloatMenuMakerMap Postfix: {e}");
            }
        }
    }
}