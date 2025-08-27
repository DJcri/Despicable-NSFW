using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(PawnRenderer), "BodyAngle")]
    public class HarmonyPatch_PawnRenderer
    {
        public static bool Prefix(ref Pawn ___pawn, ref float __result)
        {
            //set body angle to zero, for when downed
            if (___pawn?.Drawer?.renderer?.renderTree?.rootNode?.AnimationWorker is AnimationWorker_ExtendedKeyframes)
            {
                __result = 0;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    public class HarmonyPatch_PawnRenderer2
    {
        //patch so that pawns appear at the same altitude layer, at layer Pawn
        public static void Postfix(PawnRenderer __instance, ref Vector3 __result)
        {

            if (__instance.renderTree?.rootNode?.AnimationWorker is AnimationWorker_ExtendedKeyframes
                || (__instance.renderTree?.rootNode?.children is PawnRenderNode[] childNodes && childNodes.Any(x => x.AnimationWorker is AnimationWorker_ExtendedKeyframes)))
            {
                __result.y = AltitudeLayer.Pawn.AltitudeFor();
            }
        }
    }
}
