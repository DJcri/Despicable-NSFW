using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Despicable
{
    /// <summary>
    /// Handles LOGIC and contains FUNCTIONS for
    /// FINDING ANIMATIONS and ASSIGNING ROLES
    /// </summary>
    public static class ContextUtil
    {
        // Returns list of playable animation groups in context to participants
        public static List<AnimGroupDef> GetPlayableAnimationsFor(List<Pawn> participants, LovinTypeDef lovinType = null)
        {
            if (lovinType != null)
                return DefDatabase<AnimGroupDef>.AllDefsListForReading.Where(def => {
                    foreach (LovinTypeDef defLovinType in def.lovinTypes)
                    {
                        if (defLovinType == lovinType)
                            return true;
                    }
                    return false;
                }).ToList();
            return DefDatabase<AnimGroupDef>.AllDefsListForReading.ToList();
        }

        // Validates whether or not pawn fits role within animation group
        public static bool CheckPawnFitsRole(AnimRoleDef animRole, Pawn pawn)
        {
            if (PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)animRole.gender) || (animRole.gender < 1))
            {
                return true;
            }

            return false;
        }

        // Checks whether or not animation group fits the context
        public static Dictionary<string, Pawn> AssignRoles(AnimGroupDef animGroup, List<Pawn> participants)
        {
            Dictionary<string, Pawn> roleAssignments = new Dictionary<string, Pawn>();

            // Simple check
            if (participants.Count != animGroup.numActors)
            {
                return null;
            }

            // Fill specific roles first
            foreach (AnimRoleDef role in animGroup.animRoles.ToList())
            {
                if (role.gender >= 1)
                {
                    foreach (Pawn pawn in participants)
                    {
                        if (CheckPawnFitsRole(role, pawn) && !roleAssignments.ContainsValue(pawn))
                        {
                            roleAssignments.TryAdd(role.defName, pawn);
                        }
                    }
                }
            }


            // Fill flexible roles second
            foreach (AnimRoleDef role in animGroup.animRoles.ToList())
            {
                if (role.gender < 1)
                {
                    foreach (Pawn pawn in participants)
                    {
                        if (!roleAssignments.ContainsValue(pawn))
                        {
                            roleAssignments.TryAdd(role.defName, pawn);
                        }
                    }
                }
            }

            if (roleAssignments.Count < participants.Count)
            {
                return null;
            }

            return roleAssignments;
        }
    }
}
