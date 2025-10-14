using EchoColony;
using EchoColonyIntegration;
using HarmonyLib;
using RimWorld;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    // Mod class for the Despicable mod's integration with EchoColony.
    // Initializes Harmony to patch the EchoColony mod's methods.
    public class EchoColonyIntegration : Mod
    {
        public const float OPINION_CHANGE_MULTIPLIER = 0.2f;

        public EchoColonyIntegration(ModContentPack content)
            : base(content)
        {
            // Initializes and applies all Harmony patches defined in this mod.
            Harmony harmony = new Harmony("Despicable.EchoColonyIntegration");
            harmony.PatchAll();
        }
    }
}