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
            if (selectedPawns.NullOrEmpty()) return;
            InteractionMenu.InitInteractionMenu(selectedPawns[0], __result, clickPos);
        }
    }
}