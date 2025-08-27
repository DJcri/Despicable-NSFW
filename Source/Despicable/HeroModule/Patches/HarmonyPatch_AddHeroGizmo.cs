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
    public class HarmonyPatch_AddHeroGizmo
    {
        [HarmonyPriority(99), HarmonyPostfix]
        static IEnumerable<Gizmo> AddHeroGizmo(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo entry in __result)
            {
                yield return entry;
            }

            if (__instance.IsColonistPlayerControlled && CommonUtil.GetSettings().heroModuleEnabled)
            {
                // Only show gizmo if there's no chosen hero or if the pawn IS the hero
                CompHero heroComp = __instance.TryGetComp<CompHero>();
                if ((heroComp != null && heroComp.isHero)
                    || HeroUtil.FindHero() == null)
                {
                    Gizmo HeroGizmo = new HeroGizmo(__instance);
                    yield return HeroGizmo;
                }
            }
        }
    }
}
