using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Verse;

namespace Despicable
{
    /// <summary>
    /// Internal configuration for face part handling
    /// AND general HELPER FUNCTIONS for FACE PARTS
    /// </summary>

    [StaticConstructorOnStartup]
    public static class FacePartsUtil
    {
        public static readonly string TexPathBase = "FaceParts/";

        public static int expressionUpdateInterval = 60;
        public static int updateTickResetOn = 10000;
        // Blink interval should always be lower than update intervel reset on
        public static int blinkInterval = 1000;
        public static int blinkTickVariance = 240;

        public static bool IsHeadBlacklisted(HeadTypeDef headType)
        {
            // Get the Def instance.
            HeadBlacklistDef blacklistDef = DefDatabase<HeadBlacklistDef>.GetNamed("Despicable_HeadBlacklist");

            // Check if the head type is in the blacklist.
            return blacklistDef.blacklistedHeads.Contains(headType);
        }

        public static void AddHeadToBlacklist(HeadTypeDef headType)
        {
            // Get the Def instance that was loaded from your XML file.
            HeadBlacklistDef blacklistDef = DefDatabase<HeadBlacklistDef>.GetNamed("Despicable_HeadBlacklist");

            // Check if the head type is valid and not already in the list.
            if (headType == null || blacklistDef.blacklistedHeads.Contains(headType))
            {
                // Optionally log a message if the head is null or a duplicate.
                return;
            }

            // Add the head to the list. This modifies the in-memory Def instance.
            blacklistDef.blacklistedHeads.Add(headType);
            Log.Message($"Added head '{headType.defName}' to the blacklist.");
        }


        public static void RemoveHeadFromBlacklist(HeadTypeDef headType)
        {
            // Get the Def instance that was loaded from your XML file.
            HeadBlacklistDef blacklistDef = DefDatabase<HeadBlacklistDef>.GetNamed("Despicable_HeadBlacklist");

            // Check if the head type is valid and is in the list before attempting to remove it.
            if (headType == null || !blacklistDef.blacklistedHeads.Contains(headType))
            {
                // Optionally log a message if the head is null or not found.
                return;
            }

            // Remove the head from the list.
            blacklistDef.blacklistedHeads.Remove(headType);
            Log.Message($"Removed head '{headType.defName}' from the blacklist.");
        }

        public static void SaveHeadTypeBlacklist()
        {
            // Get the directory path for the config folder.
            string directoryPath = Path.Combine(Despicable.Instance.Content.RootDir, "Config");

            // Check if the directory exists and create it if it doesn't.
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Get the in-memory Def.
            HeadBlacklistDef blacklistDef = DefDatabase<HeadBlacklistDef>.GetNamed("Despicable_HeadBlacklist");
            if (blacklistDef == null)
            {
                Log.Error("Could not find the HeadBlacklistDef to save.");
                return;
            }

            // Create a data object to serialize.
            HeadBlacklistData dataToSave = new HeadBlacklistData();
            foreach (HeadTypeDef head in blacklistDef.blacklistedHeads)
            {
                dataToSave.blacklistedHeadNames.Add(head.defName);
            }

            // Correct way to get the mod's root directory
            ModContentPack modContentPack = LoadedModManager.GetMod<Despicable>().Content;
            string filePath = Path.Combine(modContentPack.RootDir, "Config", "FaceHeadBlacklist.xml");

            // Use XmlSerializer to write the data to the file.
            XmlSerializer serializer = new XmlSerializer(typeof(HeadBlacklistData));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                OmitXmlDeclaration = false
            };

            try
            {
                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    serializer.Serialize(writer, dataToSave);
                }
                ReinitializeFaceParts();
                Log.Message($"Successfully saved head blacklist to: {filePath}");
            }
            catch (System.Exception e)
            {
                Log.Error($"Failed to save head blacklist file. Error: {e.Message}");
            }
        }

        public static void LoadHeadTypeBlacklist()
        {
            // Get the directory path for the config folder.
            string directoryPath = Path.Combine(Despicable.Instance.Content.RootDir, "Config");

            // Check if the directory exists. If not, the file can't exist either.
            if (!Directory.Exists(directoryPath))
            {
                Log.Message("Config directory not found. Starting with default blacklist.");
                return;
            }

            // Get the file path.
            ModContentPack modContentPack = Despicable.Instance.Content;
            string filePath = Path.Combine(modContentPack.RootDir, "Config", "FaceHeadBlacklist.xml");

            // Check if the file exists.
            if (!File.Exists(filePath))
            {
                Log.Message("Saved blacklist file not found. Starting with default list.");
                return;
            }

            // Get the in-memory Def.
            HeadBlacklistDef blacklistDef = DefDatabase<HeadBlacklistDef>.GetNamed("Despicable_HeadBlacklist");
            if (blacklistDef == null)
            {
                Log.Error("Could not find the HeadBlacklistDef to load into.");
                return;
            }

            // Use XmlSerializer to read the data from the file.
            XmlSerializer serializer = new XmlSerializer(typeof(HeadBlacklistData));
            try
            {
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    HeadBlacklistData loadedData = (HeadBlacklistData)serializer.Deserialize(reader);

                    // Clear the existing list and add the loaded heads.
                    blacklistDef.blacklistedHeads.Clear();
                    foreach (string headName in loadedData.blacklistedHeadNames)
                    {
                        HeadTypeDef headDef = DefDatabase<HeadTypeDef>.GetNamedSilentFail(headName);
                        if (headDef != null)
                        {
                            blacklistDef.blacklistedHeads.Add(headDef);
                        }
                        else
                        {
                            Log.Warning($"Blacklisted head '{headName}' was not found in Defs and will not be loaded.");
                        }
                    }
                }
                Log.Message($"Successfully loaded head blacklist from: {filePath}");
            }
            catch (System.Exception e)
            {
                Log.Error($"Failed to load head blacklist file. Error: {e.Message}");
            }
        }

        private static void ReinitializeFaceParts()
        {
            // Reinitialize all pawns' face parts
            PawnsFinder.AllMapsAndWorld_Alive.Where(p => p.TryGetComp<CompFaceParts>() != null).ToList().ForEach(p =>
            {
                CompFaceParts comp = p.TryGetComp<CompFaceParts>();
                comp?.TryInitActions();
            });
        }
    }
}