using HarmonyLib;
using UnityEngine;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(Pawn_DrawTracker), "DrawPos", MethodType.Getter)]
    public class HarmonyPatch_DrawTracker
    {
        // We still use __instance to get the object the getter is called on.
        public static void Postfix(Pawn_DrawTracker __instance, ref Vector3 __result)
        {
            // 1. Create a Traverse object for the Pawn_DrawTracker instance.
            Traverse traverse = Traverse.Create(__instance);

            // 2. Use the 'Field' method to access the private 'pawn' field.
            // 3. Use 'GetValue<Pawn>()' to retrieve its value, cast to a Pawn.
            Pawn pawn = traverse.Field("pawn").GetValue<Pawn>();

            // The rest of your logic is now valid.
            if (pawn.TryGetComp<CompExtendedAnimator>() is CompExtendedAnimator animator)
            {
                if (animator.anchor != null && animator.anchor != pawn)
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