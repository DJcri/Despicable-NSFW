using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace Despicable
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class HarmonyPatch_AddFaceGizmo
    {
        [HarmonyPriority(100), HarmonyPostfix]
        static IEnumerable<Gizmo> AddFaceGizmo(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo entry in __result)
            {
                yield return entry;
            }

            if (__instance.IsColonistPlayerControlled && CommonUtil.GetSettings().facialPartsExtensionEnabled)
            {
                // Only show gizmo if there's no chosen hero or if the pawn IS the hero
                CompFaceParts facePartsComp = __instance.TryGetComp<CompFaceParts>();
                if (facePartsComp != null)
                {
                    Gizmo faceGizmo = new FaceGizmo(__instance);
                    yield return faceGizmo;
                }
            }
        }
    }
}
