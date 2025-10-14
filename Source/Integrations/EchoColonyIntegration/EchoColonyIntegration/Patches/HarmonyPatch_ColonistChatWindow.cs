using Despicable;
using EchoColony;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EchoColonyIntegration.Patches
{
    // Harmony patch to store the current interacting pawn.
    [HarmonyPatch(typeof(ColonistChatWindow), MethodType.Constructor, new Type[] { typeof(Pawn) })]
    public static class HarmonyPatch_ColonistChatWindow
    {
        // Postfix method that stores which pawn the hero is currently talking to in a custom CompHero component.
        public static void Postfix(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return;
                }
                Pawn heroPawn = HeroUtil.FindHero();
                if (heroPawn != null)
                {
                    CompHero comp = ((Thing)heroPawn).TryGetComp<CompHero>();
                    if (comp != null)
                    {
                        comp.pawnInteractingWith = pawn;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Despicable] - Error patching chat window constructor: " + ex.Message);
            }
        }
    }
}
