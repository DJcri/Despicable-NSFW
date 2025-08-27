using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Despicable
{
    /// <summary>
    /// [--==! TO-DO !==--]
    /// ** [ HERO MODULE ] **
    /// - Precept integration
    /// ? Hero Fast Travel System (Faction Relationship Based)
    /// ? Hero Rebirth System
    /// 
    /// ** [ Lovin Module / Animation Module ] **
    /// ? Fix pregnancy
    /// - Integrate intimacy
    /// ? Create animations for new interactions
    /// ? Implement mod extensions to KeyFrameDef to clean up code
    /// - Create textures for interaction logs
    /// 
    /// [--==! ROAD MAP !==--]
    /// - Karma precepts
    /// - Voices / AI Integration
    /// - UI Minigames ;)
    /// - Faction Relationship Based Research / Technology Gain
    /// - Threat Point / Raid / Difficulty Calculation Rebalance
    /// ? Dynamic Traits
    /// ? Dynamic Bodies
    /// ? Thirst / Water
    /// ? Advanced Role System
    /// ? Nutrition System
    /// </summary>
    public class Despicable : Mod
    {
        public static Harmony harmony;
        public const string ModName = "Despicable";
        public Settings settings;

        // Mod check bools
        public static bool nlFacialInstalled = false;

        public Despicable(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
            if (nlFacialInstalled)
                settings.facialPartsExtensionEnabled = false;

            harmony = new Harmony("com.DCSzar.Despicable");
            harmony.PatchAllUncategorized();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Debug Mode", ref settings.debugMode, "Toggle debug mode, allows mod to log messages in console");
            listingStandard.CheckboxLabeled("Animation Extension", ref settings.animationExtensionEnabled, "Toggle explicit lovin' animations (c0ffeeee's framework reworked)");
            listingStandard.CheckboxLabeled("Facial Extension", ref settings.facialPartsExtensionEnabled, "Toggle animated Despicable face parts (keep off if NL is installed)");
            listingStandard.CheckboxLabeled("Lovin' Extension", ref settings.lovinExtensionEnabled, "Toggle ability to socialize manually");
            listingStandard.CheckboxLabeled("Hero Module (Karma)", ref settings.heroModuleEnabled, "Toggle hero module");
            listingStandard.CheckboxLabeled("Nudity Enabled", ref settings.nudityEnabled, "Toggle explicit nudity (genital rendering)");
            listingStandard.Label("Sound Volume for animations");
            settings.soundVolume = listingStandard.Slider(settings.soundVolume, 0f, 1f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ModName".Translate();
        }

        static Despicable()
        {
            foreach (var mod in ModsConfig.ActiveModsInLoadOrder)
            {
                // For mod compatibility, let the mod instance know
                // Which mods are active, so it can adjust the proper settings
                // in the constructor
                switch (mod.PackageId.ToLower())
                {
                    case "nals.facialanimation":
                        nlFacialInstalled = true;
                        break;
                }
            }
        }
    }
}