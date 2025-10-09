using HarmonyLib;
using LoveyDoveySexWithRosaline;
using System;
using Verse;

/// <summary>
/// Simply replaces gender check of Despicable with
/// the GenderWorks mod's
/// </summary>

namespace Despicable
{
    public class GenderWorksIntegration : Mod
    {
        public GenderWorksIntegration(ModContentPack content)
            : base(content)
        {
            new Harmony("Despicable.GenderWorksIntegration").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PawnStateUtil), "ComparePawnGenderToByte")]
    public static class HarmonyPatch_PawnStateUtil
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, ref Pawn pawn, ref byte otherGender)
        {
            try
            {
                bool num = GenderUtilities.HasMaleReproductiveOrgan(pawn);
                bool flag = GenderUtilities.HasFemaleReproductiveOrgan(pawn);
                if (num && otherGender == 1)
                {
                    __result = true;
                }
                if (flag && otherGender == 2)
                {
                    __result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Despicable] - GenderWorksIntegration: " + ex.ToString());
            }
        }
    }
}
