using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Despicable;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class AnimationWorker_ExtendedKeyframes : AnimationWorker_Keyframes
    {
        public override bool Enabled(AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
        {
            return true;
        }

        public override Vector3 OffsetAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
        {
            return base.OffsetAtTick(tick, def, node, part, parms);
        }

        // Facial animation at tick
        public FacialAnimDef FacialAnimAtTick(int tick, AnimationPart part)
        {
            //Verse.Keyframe keyframe = this.part.keyframes[0];
            KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)part;
            Verse.Keyframe keyframe2 = keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1];
            foreach (Verse.Keyframe keyframe in keyframeAnimationPart.keyframes)
            {
                if (tick == keyframe.tick)
                {
                    return (keyframe as ExtendedKeyframe).facialAnim;
                }
            }

            return null;
        }

        public SoundDef SoundAtTick(int tick, AnimationPart part, Pawn pawn)
        {
            KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)part;
            Verse.Keyframe keyframe2 = keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1];
            foreach (Verse.Keyframe keyframe in keyframeAnimationPart.keyframes)
            {
                if (tick == keyframe.tick)
                {
                    SoundDef sound = (keyframe as ExtendedKeyframe).sound;

                    // Orgasm facial animation
                    if (LovinUtil.IsLovin(pawn))
                    {
                        CompFaceParts pawnFaceParts = pawn.TryGetComp<CompFaceParts>();
                        Pawn pawn2 = pawn.CurJob?.targetA.Pawn ?? null;
                        CompFaceParts pawn2FaceParts = null;
                        if (pawn2 != null)
                            pawn2FaceParts = pawn2.TryGetComp<CompFaceParts>() ?? null;

                        if (sound == LovinModule_SoundDefOf.Cum)
                        {
                            if (pawnFaceParts.enabled)
                            {
                                if (PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Male) && pawnFaceParts?.facialAnim != LovinModule_FacialAnimDefOf.FacialAnim_Moan)
                                    pawnFaceParts.PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Moan);
                                if (PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Female) && pawnFaceParts?.facialAnim != LovinModule_FacialAnimDefOf.FacialAnim_Orgasm)
                                    pawnFaceParts.PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Orgasm);
                            }
                            if (pawn2FaceParts.enabled)
                            {
                                if (PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Male) && pawn2FaceParts?.facialAnim != LovinModule_FacialAnimDefOf.FacialAnim_Moan)
                                    pawn2FaceParts.PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Moan);
                                if (PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Female) && pawn2FaceParts?.facialAnim != LovinModule_FacialAnimDefOf.FacialAnim_Orgasm)
                                    pawn2FaceParts.PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Orgasm);
                            }
                        }
                        else if (sound == LovinModule_SoundDefOf.Suck)
                        {
                            pawnFaceParts.PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Cheekful);
                        }
                    }

                    return sound;
                }

            }

            return null;
        }

        //use extendedkeyframes to determine addon facing
        public Rot4 FacingAtTick(int tick, AnimationPart part)
        {
            KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)part;

            //if ticks are < first keyframe tick, just be stuck to first keyframe rot
            if (tick <= keyframeAnimationPart.keyframes[0].tick)
            {

                return (keyframeAnimationPart.keyframes[0] as ExtendedKeyframe).rotation;

            }

            //if ticks are > last keyframe tick, just be stuck to last keyframe rot
            if (tick >= keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1].tick)
            {

                return (keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1] as ExtendedKeyframe).rotation;

            }
            Verse.Keyframe keyframe = keyframeAnimationPart.keyframes[0];
            Verse.Keyframe keyframe2 = keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1];
            int i = 0;
            while (i < keyframeAnimationPart.keyframes.Count)
            {
                if (tick <= keyframeAnimationPart.keyframes[i].tick)
                {
                    keyframe2 = keyframeAnimationPart.keyframes[i];
                    if (i > 0)
                    {
                        keyframe = keyframeAnimationPart.keyframes[i - 1];
                        break;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }

            return (keyframe as ExtendedKeyframe).rotation;
        }

        public bool VisibleAtTick(int tick, AnimationPart part)
        {
            KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)part;
            //if ticks are < first keyframe tick, just be stuck to first keyframe rot
            if (tick <= keyframeAnimationPart.keyframes[0].tick)
            {

                return (keyframeAnimationPart.keyframes[0] as ExtendedKeyframe).visible;

            }

            //if ticks are > last keyframe tick, just be stuck to last keyframe rot
            if (tick >= keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1].tick)
            {

                return (keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1] as ExtendedKeyframe).visible;

            }

            Verse.Keyframe keyframe = keyframeAnimationPart.keyframes[0];
            Verse.Keyframe keyframe2 = keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1];

            int i = 0;
            while (i < keyframeAnimationPart.keyframes.Count)
            {
                if (tick <= keyframeAnimationPart.keyframes[i].tick)
                {
                    keyframe2 = keyframeAnimationPart.keyframes[i];
                    if (i > 0)
                    {
                        keyframe = keyframeAnimationPart.keyframes[i - 1];
                        break;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }

            return (keyframe as ExtendedKeyframe).visible;
        }

        public virtual bool ShouldRecache(int tick, AnimationPart part)
        {
            if (FacingAtTick(tick, part) != FacingAtTick(tick - 1, part)
                || VisibleAtTick(tick, part) != VisibleAtTick(tick - 1, part)
                || VariantTexPathOnTick(tick, part) != VariantTexPathOnTick(tick - 1, part))
            {
                return true;
            }

            return true;
        }

        public int? VariantTexPathOnTick(int tick, AnimationPart part)
        {
            KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)part;

            if (tick <= keyframeAnimationPart.keyframes[0].tick)
            {
                return (keyframeAnimationPart.keyframes[0] as ExtendedKeyframe).variant;
            }

            if (tick >= keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1].tick)
            {
                return (keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1] as ExtendedKeyframe).variant;
            }

            Verse.Keyframe keyframe = keyframeAnimationPart.keyframes[0];
            Verse.Keyframe keyframe2 = keyframeAnimationPart.keyframes[keyframeAnimationPart.keyframes.Count - 1];
            int i = 0;

            while (i < keyframeAnimationPart.keyframes.Count)
            {
                if (tick <= keyframeAnimationPart.keyframes[i].tick)
                {
                    keyframe2 = keyframeAnimationPart.keyframes[i];
                    if (i > 0)
                    {
                        keyframe = keyframeAnimationPart.keyframes[i - 1];
                        break;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }

            return (keyframe as ExtendedKeyframe).variant;
        }
    }
}
