using UnityEngine;
using Verse;
using RimWorld;

namespace SelfReliefAddon
{
    public class SelfReliefSettings : ModSettings
    {
        // Настройки мода
        public static bool enableSelfRelief = true;
        public static bool requirePrivacy = true;
        public static bool respectTraits = true;
        public static float lovinThreshold = 0.3f; // При каком уровне потребности в lovin активируется
        public static float maxLovinEffect = 0.6f; // Максимум до которого может поднять lovin потребность
        public static float moodBonus = 6f; // Бонус к настроению
        public static int statusDurationHours = 3; // Длительность статуса
        public static bool debugMode = false;
        public static bool integrationMode = true;
        public static bool allowWithPartners = false; // Разрешить даже если есть партнеры на карте

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableSelfRelief, "enableSelfRelief", true);
            Scribe_Values.Look(ref requirePrivacy, "requirePrivacy", true);
            Scribe_Values.Look(ref respectTraits, "respectTraits", true);
            Scribe_Values.Look(ref lovinThreshold, "lovinThreshold", 0.3f);
            Scribe_Values.Look(ref maxLovinEffect, "maxLovinEffect", 0.6f);
            Scribe_Values.Look(ref moodBonus, "moodBonus", 6f);
            Scribe_Values.Look(ref statusDurationHours, "statusDurationHours", 3);
            Scribe_Values.Look(ref debugMode, "debugMode", false);
            Scribe_Values.Look(ref integrationMode, "integrationMode", true);
            Scribe_Values.Look(ref allowWithPartners, "allowWithPartners", false);
            
            base.ExposeData();
        }
    }

    public class SelfReliefMod : Mod
    {
        SelfReliefSettings settings;

        public SelfReliefMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SelfReliefSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            // Основные настройки
            listingStandard.Label("Self-Relief Add-on Settings (v2.0)");
            listingStandard.Label("This mod adds self-relief that works like lovin but without a partner.");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Enable Self-Relief", ref SelfReliefSettings.enableSelfRelief, 
                "Enable or disable the self-relief mechanics entirely");

            if (SelfReliefSettings.enableSelfRelief)
            {
                listingStandard.Gap();
                
                listingStandard.CheckboxLabeled("Require Privacy", ref SelfReliefSettings.requirePrivacy, 
                    "Pawns must find private locations to perform self-relief");

                listingStandard.CheckboxLabeled("Respect Character Traits", ref SelfReliefSettings.respectTraits, 
                    "Certain traits (like Ascetic) prevent self-relief");

                listingStandard.CheckboxLabeled("Enable Mod Integration", ref SelfReliefSettings.integrationMode, 
                    "Integrate with Intimacy and Despicable-NSFW mods when available");

                listingStandard.CheckboxLabeled("Allow With Available Partners", ref SelfReliefSettings.allowWithPartners, 
                    "Allow self-relief even when suitable partners are available on the map");

                listingStandard.Gap();

                // Настройки баланса
                listingStandard.Label("Balance Settings");
                
                listingStandard.Label($"Lovin Need Trigger Threshold: {SelfReliefSettings.lovinThreshold:P0}");
                SelfReliefSettings.lovinThreshold = listingStandard.Slider(SelfReliefSettings.lovinThreshold, 0.1f, 0.7f);
                listingStandard.Label("Pawns will seek self-relief when lovin need drops below this level");

                listingStandard.Gap();

                listingStandard.Label($"Maximum Lovin Effect: {SelfReliefSettings.maxLovinEffect:P0}");
                SelfReliefSettings.maxLovinEffect = listingStandard.Slider(SelfReliefSettings.maxLovinEffect, 0.4f, 0.8f);
                listingStandard.Label("Self-relief will never raise lovin need above this level");

                listingStandard.Gap();

                listingStandard.Label($"Mood Bonus: +{SelfReliefSettings.moodBonus:F0}");
                SelfReliefSettings.moodBonus = listingStandard.Slider(SelfReliefSettings.moodBonus, 2f, 12f);
                listingStandard.Label("Mood improvement from self-relief satisfaction");

                listingStandard.Gap();

                listingStandard.Label($"Status Duration: {SelfReliefSettings.statusDurationHours} hours");
                SelfReliefSettings.statusDurationHours = (int)listingStandard.Slider(SelfReliefSettings.statusDurationHours, 1, 8);
                listingStandard.Label("How long the self-relief status effects last");

                listingStandard.Gap();

                // Информационное сообщение
                GUI.color = Color.yellow;
                listingStandard.Label("IMPORTANT: This mod preserves the original 'No Lovin' debuffs.");
                listingStandard.Label("Self-relief provides partial relief but doesn't replace real intimacy.");
                GUI.color = Color.white;

                listingStandard.Gap();

                // Отладка
                listingStandard.CheckboxLabeled("Debug Mode", ref SelfReliefSettings.debugMode, 
                    "Enable debug logging for troubleshooting (shows detailed information in log)");
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Self-Relief Add-on";
        }
    }
}