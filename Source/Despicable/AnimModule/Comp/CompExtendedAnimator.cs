using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

namespace Despicable
{
    /// <summary>
    /// --==! CREDIT TO c0ffeeee !==--
    /// 
    /// This is a REWORK of their "Rimworld Animation Framework"
    /// NOT COMPATIBLE with their animations, as logic and defs have been completely rewritten.
    /// Although majority of their code pertaining to props and offsets, including patches
    /// to vanilla code remain the same.
    /// 
    /// Handles LOGIC when playing animations in a QUEUE
    /// WITH an ANCHOR and specific ROTATIONS for each node.
    /// ALSO allows LOOPING of animations
    /// </summary>
    public class CompExtendedAnimator : ThingComp
    {
        public List<AnimationDef> animQueue = new List<AnimationDef>();
        public bool hasAnimPlaying = false;
        public List<int> loopIndex = new List<int>();
        public int stage;
        public int curLoop;
        public int animationTicks;
        public Thing anchor = null;
        public int rotation;
        public Vector3 offset;

        // Main logic
        public override void CompTick()
        {
            if (!animQueue.NullOrEmpty() && hasAnimPlaying)
            {
                if (animationTicks > (animQueue[0]?.durationTicks ?? 0))
                {
                    animationTicks = 0;
                    curLoop++;
                    if (loopIndex.Count > 0)
                    {
                        if (curLoop < (loopIndex?[stage] ?? 0))
                        {
                            Loop();
                        }
                        else
                        {
                            CommonUtil.DebugLog("Playing next stage");
                            stage++;
                            PlayNext();
                        }
                    }
                    else
                    {
                        stage++;
                        PlayNext();
                    }
                }

                CheckAndPlayFacialAnim();
                CheckAndPlaySounds();
                animationTicks++;
            }

            base.CompTick();
        }

        private Pawn pawn => base.parent as Pawn;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref animQueue, "animQueue");
            Scribe_Values.Look(ref hasAnimPlaying, "hasAnimPlaying", false);
            Scribe_Collections.Look(ref loopIndex, "loopIndex");
            Scribe_Values.Look(ref stage, "stage", 0);
            Scribe_Values.Look(ref curLoop, "curLoop", 1);
            Scribe_Values.Look(ref animationTicks, "animationTicks", 0);
            Scribe_References.Look(ref anchor, "anchor");
            Scribe_Values.Look(ref rotation, "rotation", 0);
            Scribe_Values.Look(ref offset, "offset", Vector3.zero);
        }

        public void Play()
        {
            if (animQueue.NullOrEmpty())
            {
                CommonUtil.DebugLog("[Despicable] - Animation queue shouldn't be empty, something is interfering with comps, failed to play next");
            }
            else
            {
                pawn.Drawer.renderer.SetAnimation(animQueue[0]);
                hasAnimPlaying = true;
            }
        }

        public void Loop()
        {
            curLoop++;
            Play();
        }

        public void PlayNext()
        {
            if (animQueue.NullOrEmpty())
            {
                CommonUtil.DebugLog("[Despicable] - Animation queue shouldn't be empty, something is interfering with comps");
            }
            else
            {
                animQueue.RemoveAt(0);
                if (animQueue.NullOrEmpty())
                {
                    Reset();
                    return;
                }
            }

            curLoop = 1;
            Play();
        }

        public void PlayQueue(AnimGroupDef animGroupDef, List<AnimationDef> anims, AnimationOffsetDef offsetDef = null, Thing anchor = null)
        {
            Reset();

            // Apply offsets
            this.anchor = anchor;

            BaseAnimationOffset offsets = null;
            offsetDef?.FindOffset(pawn, out offsets);
            offset = offsets?.getOffset(pawn) ?? Vector3.zero;
            rotation = offsets?.getRotation(pawn) ?? 0;

            animQueue.AddRange(anims);
            loopIndex.AddRange(animGroupDef.loopIndex);
            Play();
        }

        public void Reset()
        {
            hasAnimPlaying = false;
            pawn?.Drawer?.renderer?.SetAnimation(null);
            stage = 0;
            curLoop = 1;
            animationTicks = 0;
            rotation = 0;
            anchor = null;
            offset = Vector3.zero;
            loopIndex?.Clear();
            animQueue?.Clear();
            pawn?.Drawer?.renderer?.SetAllGraphicsDirty();
        }

        public override List<PawnRenderNode> CompRenderNodes()
        {
            //only if pawn is animating for performance
            if (hasAnimPlaying)
            {
                List<PawnRenderNode> animRenderNodes = new List<PawnRenderNode>();

                // for all animationpropdefs,
                foreach (AnimationPropDef animationProp in DefDatabase<AnimationPropDef>.AllDefsListForReading)
                {
                    //if animation makes use of prop,
                    if (AnimationMakesUseOfProp(animationProp))
                    {
                        PawnRenderNodeProperties props = animationProp.animPropProperties;
                        if (props.texPath.NullOrEmpty())
                        {
                            props.texPath = "AnimationProps/MissingTexture/MissingTexture";
                        }

                        //create new render node
                        PawnRenderNode animRenderNode = CommonUtil.CreateNode(pawn, props);
                        animRenderNodes.Add(animRenderNode);
                    }
                }

                //return list of rendernodes that should animate
                return animRenderNodes;
            }
            else
            {
                return null;
            }
        }

        public bool AnimationMakesUseOfProp(AnimationPropDef animationProp)
        {
            // never true if not animating; anim props shouldn't be attached
            if (!hasAnimPlaying) return false;

            //for all anims in queue (because it's only recached at start)
            foreach (AnimationDef animation in animQueue)
            {
                foreach (PawnRenderNodeTagDef propTag in animation.keyframeParts.Keys)
                {
                    // if that proptag is the same as the one for animationProp,
                    if (propTag == animationProp.animPropProperties.tagDef)
                    {
                        //that prop is being used in the animation
                        return true;
                    }
                }
            }

            return false;
        }

        public void CheckAndPlayFacialAnim()
        {
            PawnRenderNode rootNode = pawn.Drawer?.renderer?.renderTree?.rootNode;

            // Null checks for mid-save compatibility
            // and general stability
            if (rootNode == null) return;
            if (rootNode.AnimationWorker == null) return;

            if (rootNode.AnimationWorker is AnimationWorker_ExtendedKeyframes animWorker)
            {
                AnimationPart animPart;
                rootNode.tree.TryGetAnimationPartForNode(rootNode, out animPart);

                // Don't check for facial animation if there is no anim part
                if (animPart == null) return;

                FacialAnimDef facialAnim = animWorker.FacialAnimAtTick(rootNode.tree.AnimationTick, animPart);

                if (facialAnim != null)
                {
                    CompFaceParts facePartsComp = pawn.TryGetComp<CompFaceParts>();
                    if (facePartsComp != null)
                    {
                        facePartsComp.PlayFacialAnim(facialAnim);
                    }
                }
            }
        }

        public void CheckAndPlaySounds()
        {
            // Just in case settings are somehow not loaded at this point, use 1f
            float volumeSetting = CommonUtil.GetSettings()?.soundVolume ?? 1f;
            PawnRenderNode rootNode = pawn.Drawer?.renderer?.renderTree?.rootNode;

            // Null checks for mid save compatibility
            if (rootNode == null) return;
            if (rootNode.AnimationWorker == null) return;

            // Check if rootnode has sound to play at this tick
            if (rootNode?.AnimationWorker is AnimationWorker_ExtendedKeyframes animWorker)
            {
                if (rootNode.tree != null)
                {
                    AnimationPart animPart;
                    rootNode.tree.TryGetAnimationPartForNode(rootNode, out animPart);

                    // Don't check for sound if there is no anim part
                    if (animPart == null) return;

                    SoundDef sound = animWorker.SoundAtTick(rootNode.tree.AnimationTick, animPart, pawn) ?? null;

                    if (sound != null)
                    {
                        SoundInfo soundInfo = new TargetInfo(pawn.Position, pawn.Map);

                        soundInfo.volumeFactor = volumeSetting;
                        sound.PlayOneShot(soundInfo);
                    }
                }
            }
        }
    }
}