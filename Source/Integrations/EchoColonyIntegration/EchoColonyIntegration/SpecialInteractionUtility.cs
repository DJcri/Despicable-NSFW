using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

/// <summary>
/// Functions for special chat interactions that don't have corresponding 
/// basegame interactions.
/// </summary>
namespace EchoColonyIntegration
{
    public static class SpecialInteractionUtility
    {
        // Handle a recruitment attempt interaction
        // Function takes in success bool from AI response
        public static void HandleRecruitmentAttempt(Pawn heroPawn, Pawn pawnInteractingWith, bool success)
        {
            if (success)
            {
                // If successful, attempt to recruit the pawn
                if (pawnInteractingWith.Faction != Faction.OfPlayer)
                {
                    RecruitUtility.Recruit(pawnInteractingWith, heroPawn.Faction, heroPawn);
                }
            }
        }

        // Handle an attempt to convince a raider to end the raid
        public static void HandleConvinceToEndRaidAttempt(Pawn heroPawn, Pawn pawnInteractingWith, bool success)
        {
            if (success)
            {
                // If successful, end the raid
                if (pawnInteractingWith.Faction != Faction.OfPlayer && pawnInteractingWith.HostileTo(Faction.OfPlayer))
                {
                    Map map = heroPawn.Map;
                    if (map != null)
                    {
                        EndRaid(map);
                    }
                }
            }
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
    }
}
