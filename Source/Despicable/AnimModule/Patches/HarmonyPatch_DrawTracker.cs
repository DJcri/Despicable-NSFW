using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(Pawn_DrawTracker), "DrawPos", MethodType.Getter)]
    public class HarmonyPatch_DrawTracker
    {
        public static void Postfix(ref Pawn ___pawn, ref Vector3 __result)
        {
            if (___pawn.TryGetComp<CompExtendedAnimator>() is CompExtendedAnimator animator)
            {
                if (animator.anchor != null && animator.anchor != ___pawn)
                {
                    // Anchor pawn, offset from anchor
                    Vector3 anchorPos = animator.anchor.DrawPos;
                    __result.x = anchorPos.x;
                    __result.z = anchorPos.z;
                }
            }
        }
    }
}
