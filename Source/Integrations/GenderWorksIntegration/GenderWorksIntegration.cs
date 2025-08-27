using HarmonyLib;
using LoveyDoveySexWithRosaline;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class GenderWorksIntegration : Mod
    {
        public GenderWorksIntegration(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Despicable.GenderWorksIntegration");
            harmony.PatchAll();
        }
    }

    // Use Gender Works' gender assignment checks to determine genders
    // and availability for interactions / animations
    [HarmonyPatch(typeof(PawnStateUtil), "ComparePawnGenderToByte")]
    static class HarmonyPatch_PawnStateUtil
    {
        public static void Postfix(ref bool __result, ref Pawn pawn, ref byte otherGender)
        {
            CommonUtil.DebugLog("[Gender Works] - Running gender check");

            try
            {
                bool hasPenis = LoveyDoveySexWithRosaline.GenderUtilities.HasMaleReproductiveOrgan(pawn);
                bool hasVagina = LoveyDoveySexWithRosaline.GenderUtilities.HasFemaleReproductiveOrgan(pawn);

                if (hasPenis && otherGender == 1)
                    __result = true;
                if (hasVagina && otherGender == 2)
                    __result = true;
            }
            catch (Exception e)
            {
                CommonUtil.DebugLog(e.ToString());
            }
        }
    }
}