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
    public class EchoColonyIntegration : Mod
    {
        public EchoColonyIntegration(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Despicable.IntimacyIntegration");
            harmony.PatchAll();
        }
    }

    // Modify the context prompt
    [HarmonyPatch(typeof(LovinUtil), "PassesLovinCheck")]
    static class HarmonyPatch_LovinUtil
    {
        public static void Postfix(ref bool __result, ref Pawn pawn, ref Pawn target)
        {
            __result = true;

            if (!SexUtilities.CanDoLovin(pawn) || !SexUtilities.CanDoLovin(target)
                || (!SocialInteractionUtility.CanInitiateInteraction(pawn))
                || (!CommonChecks.IsOldEnough(pawn) || !CommonChecks.IsOldEnough(target))
                || (!CommonChecks.AreMutuallyAttracted(pawn, target))
                || (CommonChecks.IdeologyForbidsLovin(pawn, target))
                || (CommonChecks.IncestCheck(pawn, target)))
            {
                __result = false;
            }
        }
    }

    // Modify OnResponse() to update the hero player's relationship with the pawn
}
