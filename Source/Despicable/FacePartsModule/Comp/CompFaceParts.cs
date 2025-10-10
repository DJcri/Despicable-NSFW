using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Despicable
{
    /// <summary>
    /// Handles logic of RENDERING FACES and FACIAL ANIMATION
    /// Very badly written, I know, but the whole system needs a rewrite.
    /// A rewrite would be insanely complex and time consuming,
    /// minor patches/changes should suffice.
    /// </summary>

    public class CompFaceParts : ThingComp
    {
        // Default constants
        public static readonly string DEFAULT_GENDER_PATH = "Male/";

        // State variables
        public bool enabled;
        public bool shouldUpdate = false;
        public int ticks = 0;
        public static int blinkTickVariance = FacePartsUtil.blinkTickVariance;
        public int blinkTick = FacePartsUtil.blinkInterval + Rand.Range(-blinkTickVariance, blinkTickVariance);

        // Style paths
        public string genderPath = DEFAULT_GENDER_PATH;
        public FacePartStyleDef eyeStyleDef = null;
        public FacePartStyleDef mouthStyleDef = null;

        // Animation
        public ExpressionDef baseExpression;
        public ExpressionDef animExpression;
        public FacialAnimDef facialAnim;
        public int curKeyframe = 0;
        public int animTicks = 0;

        public override void CompTick()
        {
            // Catch error when ticking to prevent completely breaking
            // mid save
            try
            {
                if (enabled)
                {
                    if (facialAnim != null)
                    {
                        if (!facialAnim.keyframes.NullOrEmpty())
                        {
                            if (animExpression == null)
                            {
                                animExpression = new ExpressionDef();
                            }

                            // Set facial expression for current keyframe
                            for (int i = 0; i < facialAnim.keyframes.Count; i++)
                            {
                                FacialAnimKeyframeDef keyframe = facialAnim.keyframes[i];
                                FacialAnimKeyframeDef lastKeyframe = null;
                                if (i > 0)
                                {
                                    lastKeyframe = facialAnim.keyframes[i - 1];
                                }
                                if (animTicks == keyframe.tick)
                                {
                                    // If not set in cur keyframe, stick to last used tex path for face part
                                    // If offset isn't set in next keyframe, reset to zero offset
                                    animExpression.eyesOffset = keyframe.expression.eyesOffset ?? Vector3.zero;
                                    animExpression.mouthOffset = keyframe.expression.mouthOffset ?? Vector3.zero;
                                    animExpression.texPathEyes = keyframe.expression.texPathEyes ?? lastKeyframe?.expression.texPathEyes ?? null;
                                    animExpression.texPathMouth = keyframe.expression.texPathMouth ?? lastKeyframe?.expression.texPathMouth ?? null;
                                    animExpression.texPathDetail = keyframe.expression.texPathDetail ?? lastKeyframe?.expression.texPathDetail ?? null;

                                    shouldUpdate = true;
                                    break;
                                }
                            }
                        }

                        animTicks++;
                        // If animation finishes, reset
                        if (animTicks > facialAnim.durationTicks)
                        {
                            facialAnim = null;
                            animExpression = null;
                            animTicks = 0;
                            shouldUpdate = true;
                        }
                    }
                    // Blinking animation if facial animation isn't already playing
                    else if (ticks > 0)
                    {
                        if (blinkTick != 0 && FacePartsUtil.expressionUpdateInterval != 0)
                        {
                            if (ticks % blinkTick == 0 && !PawnStateUtil.IsAsleep(pawn))
                            {
                                if (LovinUtil.IsLovin(pawn))
                                {
                                    if (Rand.Bool)
                                        PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Moan);
                                    else
                                        PlayFacialAnim(LovinModule_FacialAnimDefOf.FacialAnim_Gasp);
                                }
                                else
                                {
                                    // Babies drool sometimes
                                    if (PawnStateUtil.isInfant(pawn) && Rand.Bool)
                                        PlayFacialAnim(FacePartsModule_FacialAnimDefOf.FacialAnim_Drool);

                                    // Everyone needs to blink
                                    else
                                        PlayFacialAnim(FacePartsModule_FacialAnimDefOf.FacialAnim_Blink);
                                }

                                enabled = CommonUtil.GetSettings().facialPartsExtensionEnabled;
                                shouldUpdate = true;

                                // Assign the next tick to blink randomly
                                blinkTick = FacePartsUtil.blinkInterval + Rand.Range(-blinkTickVariance, blinkTickVariance);
                            }

                            if (ticks % FacePartsUtil.expressionUpdateInterval == 0)
                            {
                                /// Int shouldn't get too big over long play times
                                /// this value is also saved, so it should be reset periodically
                                if (ticks > FacePartsUtil.updateTickResetOn)
                                    ticks = 0;
                                shouldUpdate = true;
                            }
                        }
                    }
                }

                if (shouldUpdate)
                {
                    shouldUpdate = false;

                    // Update graphics
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                }
            }
            catch (Exception e)
            {
                Log.Error($"[Despicable] - Error in CompFaceParts CompTick: {e}");
                enabled = false;
            }

            ticks++;
            base.CompTick();
        }

        // Check at initialization whether to enable faces
        // (Settings check if NL is installed and turns off my faces if they are)
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            TryInitActions();
            enabled = CommonUtil.GetSettings()?.facialPartsExtensionEnabled ?? false;
        }

        public void TryInitActions()
        {
            // Determine gender path
            genderPath = PawnStateUtil.ComparePawnGenderToByte(pawn, (byte)Gender.Female) ? "Female/" : DEFAULT_GENDER_PATH;

            // Check if enabled, if not, continue to do nothing for performance
            if (!enabled)
                return;

            // Alpha gene headtypes are not designed for faces to be plastered on them
            // So disable them here
            HeadTypeDef headType = pawn?.story?.headType;
            if (headType == null)
                return;

            if (FacePartsUtil.IsHeadBlacklisted(headType))
            {
                enabled = false;
                return;
            }

            if (headType.defName.ToLower().StartsWith("ag_"))
            {
                enabled = false;
                return;
            }
        }

        public void AssignStylesRandomByWeight()
        {
            // Get lists of styles by face part type
            List<FacePartStyleDef> eyeStyles = DefDatabase<FacePartStyleDef>.AllDefsListForReading.Where(s =>
            {
                if (s.renderNodeTag.defName != "FacePart_Eye")
                    return false;

                if (s.requiredGender != null)
                {
                    if (s.requiredGender != (byte)pawn.gender)
                    {
                        return false;
                    }
                }
                return true;

            }).ToList();
            List<FacePartStyleDef> mouthStyles = DefDatabase<FacePartStyleDef>.AllDefsListForReading.Where(s =>
            {
                if (s.renderNodeTag.defName != "FacePart_Mouth")
                    return false;

                if (s.requiredGender != null)
                {
                    if (s.requiredGender != (byte)pawn.gender)
                    {
                        return false;
                    }
                }
                return true;

            }).ToList();

            // Total weight for each face part type
            int totalEyeWeight = eyeStyles.Sum(s => s.weight);
            int totalMouthWeight = mouthStyles.Sum(s => s.weight);

            // Randomly select eye style based on weight
            // Roll a random number between 0 and total weight
            int eyeRoll = Rand.Range(0, totalEyeWeight);
            foreach (var item in eyeStyles)
            {
                eyeRoll -= item.weight;
                if (eyeRoll <= 0)
                {
                    eyeStyleDef = item;
                    break;
                }
            }

            // Do the same for mouth style
            int mouthRoll = Rand.Range(0, totalMouthWeight);
            foreach (var item in mouthStyles)
            {
                mouthRoll -= item.weight;
                if (mouthRoll <= 0)
                {
                    mouthStyleDef = item;
                    break;
                }
            }

            shouldUpdate = true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Values.Look(ref blinkTick, "blinkTick");
            Scribe_Values.Look(ref genderPath, "genderPath", DEFAULT_GENDER_PATH);
            Scribe_Defs.Look(ref eyeStyleDef, "eyeStyleDef");
            Scribe_Defs.Look(ref mouthStyleDef, "mouthStyleDef");
            Scribe_Defs.Look(ref baseExpression, "baseExpression");
            Scribe_Defs.Look(ref animExpression, "animExpression");
            Scribe_Defs.Look(ref facialAnim, "facialAnim");
            Scribe_Values.Look(ref animTicks, "animTicks", 0);
        }

        private Pawn pawn => parent as Pawn;

        public void PlayFacialAnim(FacialAnimDef anim)
        {
            if (anim == null)
                return;
            if (pawn == null)
                return;
            // For things that shouldn't animate faces
            if (pawn.Dead)
                return;
            if (pawn?.pather == null)
                return;
            if (pawn?.pather?.debugDisabled == true)
                return;

            facialAnim = anim;
        }

        public override List<PawnRenderNode> CompRenderNodes()
        {
            // Assign styles if not already assigned
            if (ticks > 0 && (mouthStyleDef == null || eyeStyleDef == null))
                AssignStylesRandomByWeight();

            List<PawnRenderNode> facePartNodes = new List<PawnRenderNode>();

            if (pawn == null)
                return null;
            if (!pawn.RaceProps.Humanlike || !enabled)
                return null;
            if (!enabled)
                return null;

            //** Conditional rendering (drunk, high, tired, happy... etc.)
            // Changes the base facial expression of pawn, not prioritized
            // over animation expression
            baseExpression = null;

            PawnRenderNodeProperties detailProps = DefDatabase<FacePartDef>.GetNamed("FacePart_Detail_L").properties;
            detailProps.texPath = "FaceParts/Details/detail_empty";

            // Create symmetrical nodes for details
            PawnRenderNode detailNode = CommonUtil.CreateNode(pawn, detailProps, PawnRenderNodeTagDefOf.Head);
            detailProps = DefDatabase<FacePartDef>.GetNamed("FacePart_Detail_R").properties;
            detailProps.texPath = detailNode.Props.texPath;
            PawnRenderNode detailNodeMirror = CommonUtil.CreateNode(pawn, detailProps, PawnRenderNodeTagDefOf.Head);

            facePartNodes.Add(detailNode);
            facePartNodes.Add(detailNodeMirror);

            // Render using animation first, conditional second, style last
            foreach (FacePartDef facePartDef in DefDatabase<FacePartDef>.AllDefsListForReading.ToList())
            {
                switch (facePartDef.properties.debugLabel)
                {
                    case "FacePart_Eye_L":
                    case "FacePart_Eye_R":
                        facePartDef.properties.texPath = FacePartsUtil.GetEyePath(pawn, facePartDef.properties.texPath);
                        break;
                }

                try
                {
                    PawnRenderNodeProperties facePartProps = facePartDef.properties;

                    // Don't render if nothing to render
                    if (facePartProps.texPath.NullOrEmpty() || facePartProps.texPath.StartsWith("Gendered/"))
                    {
                        continue;
                    }

                    //** Make sure the graphics are flipped correctly (IF FACING SOUTH!)
                    /// flipping the textures to try to mirror the left half will provide the opposite effect
                    /// since, by default if a pawn faces west all textures are FLIPPED ALREADY
                    /// SO ONLY FLIP WHEN MIRRORING WHILE FACING THE CAMERA
                    if (pawn.Rotation != Rot4.East && pawn.Rotation != Rot4.West)
                    {
                        if (facePartProps.debugLabel.ToLower().EndsWith("_r"))
                        {
                            facePartProps.flipGraphic = true;
                        }
                    }

                    // Prevent face part from rendering on the back side of the pawn's head when flipped 
                    facePartProps.oppositeFacingLayerWhenFlipped = false;

                    PawnRenderNode facePartNode = CommonUtil.CreateNode(pawn, facePartProps, PawnRenderNodeTagDefOf.Head);
                    facePartNodes.Add(facePartNode);
                }
                catch (Exception e)
                {
                    Log.Error($"[Despicable] - Error in CompFaceParts CompRenderNodes for {facePartDef.defName}: {e}");
                    continue;
                }
            }

            return facePartNodes;
        }
    }
}
