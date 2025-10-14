using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace Despicable
{
    [StaticConstructorOnStartup]
    public static class CommonUtil
    {
        // [General Utilities]
        public static Settings GetSettings()
        {
            var mod = Despicable.Instance;
            if (mod == null)
            {
                // Return a default settings object to prevent NullReferenceException
                return new Settings();
            }
            return mod.GetSettings<Settings>();
        }

        public static void DebugLog(string msg)
        {
            if (GetSettings().debugMode)
                Log.Message(msg);
        }

        // Helper function to end an ongoing raid on the map
        public static void EndRaid(Map map)
        {
            // Iterate through all Lord groups on the map
            foreach (Lord lord in map.lordManager.lords.ToList())
            {
                // Check if the Lord's job is a hostile one (like a raid or siege)
                // This is the most important part: identifying the hostile Lords.
                if (lord.LordJob is LordJob_AssaultColony ||
                    lord.LordJob is LordJob_Siege ||
                    lord.LordJob is LordJob_DefendBase) // Include other raid types if necessary
                {
                    // Forcing the lord to end will either cause the enemies to flee,
                    // or sometimes result in a game-defined "raid victory" condition.
                    // LordToil_AssaultColony.ForceFinished() is another, sometimes cleaner, option
                    // but simply removing the lord usually works.

                    // Use lord.Cleanup() to remove the lord from the manager.
                    // This usually triggers the appropriate LordJob exit logic (e.g., fleeing).
                    lord.Cleanup();
                }
            }
        }

        // [Rendering]
        public static PawnRenderNode CreateNode(Pawn pawn, PawnRenderNodeProperties props, PawnRenderNodeTagDef parentTag = null)
        {
            if (parentTag != null)
            {
                props.parentTagDef = parentTag;
            }

            PawnRenderNode newNode = (PawnRenderNode)Activator.CreateInstance(props.nodeClass, new object[] {
                pawn,
                props,
                pawn.Drawer.renderer.renderTree
            });

            return newNode;
        }
    }
}
