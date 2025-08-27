using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    /// <summary>
    /// ANIMATION REMOTE CONTROL
    /// </summary>
    public static class AnimUtil
    {
        public static void PlayAnimationGroup(AnimGroupDef animGroupDef, Dictionary<string, Pawn> roleAssignments, Thing anchor = null)
        {
            foreach (string animRoleDefName in roleAssignments.Keys)
            {
                // Add role animations to queue for actors
                AnimRoleDef animRole = DefDatabase<AnimRoleDef>.GetNamed(animRoleDefName);
                Pawn pawn = roleAssignments[animRoleDefName];

                pawn.TryGetComp(out CompExtendedAnimator animator);
                animator.PlayQueue(animGroupDef, animRole.anims.ToList(), animRole.offsetDef, anchor);
            }
        }

        public static void ResetAnimatorsForGroup(List<Pawn> pawns)
        {
            foreach (Pawn pawn in pawns)
            {
                CompExtendedAnimator animator = pawn.GetComp<CompExtendedAnimator>();
                animator.Reset();
            }
        }

        /// <summary>
        /// Simple play animation function for debugging/testing purposes
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="defName"></param>
        public static void PlayByAnimDefName(Pawn pawn, string defName)
        {
            pawn.Drawer.renderer.SetAnimation(DefDatabase<AnimationDef>.GetNamed(defName));
        }
    }
}
