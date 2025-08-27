using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Despicable
{
    public class Dialog_AnimBuilder : Window
    {
        // Constants
        public override Vector2 InitialSize => new Vector2(950f, 750f);
        private static Vector2 ButSize = new Vector2(150f, 30f);
        private static readonly float ButtonGap = 4f;
        private static readonly List<PawnRenderNodeTagDef> AnimatableRenderNodes = new List<PawnRenderNodeTagDef>()
        {
            PawnRenderNodeTagDefOf.Head,
            PawnRenderNodeTagDefOf.Body,
            LovinModule_PawnRenderNodeTagDefOf.Genitals
        };

        public List<Actor> actors = new List<Actor>();
        public Actor actor;

        // Add a new actor
        public void AddActor()
        {
            Reset(true);
            actor = new Actor(roleName, gender);
            actors.Add(actor);
        }

        public string roleName = "Role_Name";
        public int gender = 0;
        public string genderBuffer = "0";
        public bool addSound = false;
        public int tick = 0;
        public string tickBuffer = "0";
        public int loopCount = 1;
        public string loopCountBuffer = "1";
        public int stage = 0;
        public int durationTicks = 0;
        public string durationTicksBuffer = "0";
        public Dictionary<PawnRenderNodeTagDef, Vector3> offsets = new Dictionary<PawnRenderNodeTagDef, Vector3>();

        public void CreateKeyFrame()
        {
            ExtendedKeyframe newKeyframe = new ExtendedKeyframe();
            newKeyframe.tick = tick;
            newKeyframe.angle = actor.angle;
            newKeyframe.visible = actor.visible;
            newKeyframe.rotation = actor.rotation;
            newKeyframe.sound = DefDatabase<SoundDef>.GetNamed(actor.soundDef);

            // Add new keyframe to current animation and remove existing if applicable
            List<Verse.Keyframe> existingKeyframes = actor.keyframeParts[actor.nodeSelected].keyframes;
            foreach (var otherKeyframe in existingKeyframes)
            {
                if (otherKeyframe.tick == tick)
                    existingKeyframes.Remove(otherKeyframe);
            }

            actor.keyframeParts[actor.nodeSelected].keyframes.Add(newKeyframe);
            addSound = false;
        }

        public void CreateAnimationForAllActors()
        {
            foreach (var a in actors)
            {
                AnimationDef newAnimDef = new AnimationDef();
                newAnimDef.defName = a.animName + $"{a.roleName}_stage{stage}";
                newAnimDef.durationTicks = durationTicks;
                newAnimDef.keyframeParts = a.keyframeParts;

                // Add animation to role
                a.roleAnimations.Add(newAnimDef);
                a.OnNewStage();
            }

            // Add loopCount to loop table
            loopIndex.Add(loopCount);

            // Move on to create next stage
            // Reset tick (and offset?)
            tick = 0;
            stage++;
            durationTicks = 0;
            durationTicksBuffer = "0";
        }

        // Create an animation group
        public string animGroupName;
        public Dictionary<LovinTypeDef, bool> lovinTypes = new Dictionary<LovinTypeDef, bool>();
        public List<AnimRoleDef> animRoleDefs;
        public List<int> loopIndex = new List<int>();

        public void CreateAnimationGroup()
        {
            foreach (var a in actors)
            {
                AnimRoleDef animRoleDef = new AnimRoleDef();
                animRoleDef.defName = a.roleName;
                animRoleDef.gender = a.gender;
                animRoleDef.anims = a.roleAnimations;

                // Add role to animation group
                animRoleDefs.Add(animRoleDef);
            }

            AnimGroupDef newAnimGroupDef = new AnimGroupDef();
            newAnimGroupDef.defName = animGroupName;
            newAnimGroupDef.numActors = actors.Count;

            // Write to xml
        }

        // Reset to create a new animation or start over
        public void Reset(bool addingNewActor = false)
        {
            if (!actors.NullOrEmpty())
            {
                if (!addingNewActor)
                    actors.Clear();
                else
                {
                    foreach (var a in actors)
                    {
                        a.Reset();
                    }
                }
            }

            stage = 0;
            tick = 0;
            tickBuffer = "0";
            loopCount = 1;
            loopCountBuffer = "1";
            if (!loopIndex.NullOrEmpty())
                loopIndex.Clear();
            if (!animRoleDefs.NullOrEmpty())
                animRoleDefs.Clear();
            if (!lovinTypes.NullOrEmpty())
                lovinTypes.Clear();
        }

        // UI
        public override void DoWindowContents(Rect inRect)
        {
            CreateButtons(inRect);
        }

        public void CreateButtons(Rect inRect)
        {
            Text.Font = GameFont.Small;
            float yPos = 0f;

            // Text field for role name
            Rect roleNameField = new Rect(inRect);
            roleNameField.width = ButSize.x;
            roleNameField.height = ButSize.y;
            try
            {
                string fieldText = Widgets.TextField(roleNameField, roleName);
                roleName = fieldText;
            }
            catch(Exception e)
            {
                CommonUtil.DebugLog(e.ToString());
            }

            // Text field to set gender
            Rect genderField = new Rect(inRect);
            genderField.width = ButSize.x;
            genderField.height = ButSize.y;
            genderField.x = ButSize.x + ButtonGap;
            Widgets.TextFieldNumericLabeled(genderField, "Gender: ", ref gender, ref genderBuffer);

            // Button to add actor
            Rect addActorButton = new Rect(inRect);
            addActorButton.width = ButSize.x;
            addActorButton.height = ButSize.y;
            addActorButton.x += genderField.xMax + ButtonGap;
            if (Widgets.ButtonText(addActorButton, "Add new actor role"))
            {
                AddActor();
            }

            // Render buttons to choose actor
            if (!actors.NullOrEmpty())
                for (int i = 0; i < actors.Count; i++)
                {
                    Rect selectActorButton = new Rect(inRect);
                    selectActorButton.width = ButSize.x;
                    selectActorButton.height = ButSize.y;
                    selectActorButton.x += addActorButton.xMax + ButtonGap + (ButSize.x + ButtonGap) * i;
                    if (Widgets.ButtonText(selectActorButton, actors[i].roleName))
                    {
                        if (actor == actors[i])
                            actor = null;
                        else actor = actors[i];
                    }

                    if (actor == actors[i])
                        Widgets.DrawStrongHighlight(selectActorButton);
                }

            if (actor == null)
                return;

            // Text field to set duration
            Rect durationTicksField = new Rect(inRect);
            durationTicksField.width = ButSize.x;
            durationTicksField.height = ButSize.y;
            durationTicksField.y = addActorButton.yMax + ButtonGap + 8f;
            Widgets.TextFieldNumericLabeled(durationTicksField, "Duration: ", ref durationTicks, ref durationTicksBuffer);

            // Text field to set tick
            Rect tickField = new Rect(inRect);
            tickField.width = ButSize.x;
            tickField.height = ButSize.y;
            tickField.x = durationTicksField.xMax + ButtonGap;
            tickField.y = durationTicksField.y;
            Widgets.TextFieldNumericLabeled(tickField, "Tick: ", ref tick, ref tickBuffer);

            // List of buttons to select node
            for (int i = 0; i < AnimatableRenderNodes.Count; i++)
            {
                Rect button = new Rect(inRect);
                button.width = ButSize.x;
                button.height = ButSize.y;
                button.x += (ButSize.x + ButtonGap) * i;
                button.y = tickField.yMax + ButtonGap + 32f;
                if (Widgets.ButtonText(button, AnimatableRenderNodes[i].defName))
                {
                    if (actor.nodeSelected == AnimatableRenderNodes[i])
                    {
                        actor.nodeSelected = null;
                    }
                    else
                    {
                        actor.nodeSelected = AnimatableRenderNodes[i];
                    }
                }

                if (actor.nodeSelected != null)
                {
                    if (actor.nodeSelected == AnimatableRenderNodes[i])
                    {
                        Widgets.DrawStrongHighlight(button);
                    }
                }

                yPos = button.yMax;
            }

            if (actor.nodeSelected != null)
            {
                // Buttons to set rotation
                int roti = 0; /// index
                foreach (var rot in Rot4.AllRotations.ToList())
                {
                    Rect button = new Rect(inRect);
                    button.width = ButSize.x / 1.5f;
                    button.height = ButSize.y;
                    button.x += (ButSize.x / 1.5f + ButtonGap) * roti;
                    button.y = yPos + ButtonGap * 2;
                    if (Widgets.ButtonText(button, rot.ToStringWord()))
                    {
                        actor.rotation = rot;
                    }

                    if (actor.rotation == rot)
                        Widgets.DrawStrongHighlight(button);

                    roti++;
                    if (roti == 4)
                        yPos = button.yMax;
                }

                // Text field to set angle
                Rect angleField = new Rect(inRect);
                angleField.width = ButSize.x;
                angleField.height = ButSize.y;
                angleField.y = yPos + ButtonGap;
                Widgets.TextFieldNumericLabeled(angleField, "Angle: ", ref actor.angle, ref actor.angleBuffer);

                // Checkbox to toggle visibility
                Rect visibilityCheckbox = new Rect(inRect);
                visibilityCheckbox.width = ButSize.x;
                visibilityCheckbox.height = ButSize.y;
                visibilityCheckbox.y = angleField.y + ButSize.y + ButtonGap;
                Widgets.CheckboxLabeled(visibilityCheckbox, "Toggle visibile", ref actor.visible);

                // Checkbox to toggle adding sound to keyframe
                Rect soundCheckbox = new Rect(inRect);
                soundCheckbox.width = ButSize.x;
                soundCheckbox.height = ButSize.y;
                soundCheckbox.y = visibilityCheckbox.yMax + ButtonGap;
                Widgets.CheckboxLabeled(soundCheckbox, "Add sound", ref addSound);

                // Text field to set sound def
                Rect soundDefField = new Rect(inRect);
                soundDefField.width = ButSize.x;
                soundDefField.height = ButSize.y;
                soundDefField.x = soundCheckbox.xMax + ButtonGap;
                soundDefField.y = soundCheckbox.y;
                try
                {
                    string fieldText = Widgets.TextField(soundDefField, actor.soundDef);
                    actor.soundDef = fieldText;
                }
                catch (Exception e)
                {
                    CommonUtil.DebugLog(e.ToString());
                }

                yPos = soundDefField.yMax;
            }

            // Button to add keyframe to animation
            Rect createKeyframeButton = new Rect(inRect);
            createKeyframeButton.width = ButSize.x;
            createKeyframeButton.height = ButSize.y;
            createKeyframeButton.y = yPos + ButtonGap;
            if (Widgets.ButtonText(createKeyframeButton, "Add keyframe for actor"))
            {
                CreateKeyFrame();
            }

            // Text field to set number of loops for stage
            Rect loopCountField = new Rect(inRect);
            loopCountField.width = ButSize.x;
            loopCountField.height = ButSize.y;
            loopCountField.y = createKeyframeButton.yMax + ButtonGap + 32f;
            Widgets.TextFieldNumericLabeled(loopCountField, "Loops: ", ref loopCount, ref loopCountBuffer);

            // Button to add animation to role
            Rect createAnimationButton = new Rect(inRect);
            createAnimationButton.width = ButSize.x;
            createAnimationButton.height = ButSize.y;
            createAnimationButton.y = loopCountField.yMax + ButtonGap;
            if (Widgets.ButtonText(createAnimationButton, "Add stage"))
            {
                CreateAnimationForAllActors();
            }

            yPos = createAnimationButton.yMax + 32f;

            // List of checkboxes to toggle lovin types appropriate for animation
            if (lovinTypes.NullOrEmpty())
            {
                foreach (var lovinType in DefDatabase<LovinTypeDef>.AllDefsListForReading.ToList())
                {
                    lovinTypes.Add(lovinType, false);
                }
            }

            if (!lovinTypes.NullOrEmpty())
            {
                foreach (var key in lovinTypes.Keys)
                {
                    Rect lovinTypeToggleButton = new Rect(inRect);
                    lovinTypeToggleButton.width = ButSize.x;
                    lovinTypeToggleButton.height = ButSize.y;
                    lovinTypeToggleButton.y = yPos + ButtonGap;
                    if (Widgets.ButtonText(lovinTypeToggleButton, key.defName))
                    {
                        lovinTypes[key] = !lovinTypes[key];
                    }

                    if (lovinTypes[key])
                        Widgets.DrawStrongHighlight(lovinTypeToggleButton);

                    yPos = lovinTypeToggleButton.yMax;
                }
            }

            // Validate here
            bool valid = true;
            // Make sure role names are not the same

            if (valid)
            {
                // Button to add role to animation "group"
                Rect finalizeButton = new Rect(inRect);
                finalizeButton.width = ButSize.x;
                finalizeButton.height = ButSize.y;
                finalizeButton.y = yPos + ButtonGap;
                if (Widgets.ButtonText(finalizeButton, "Finalize"))
                {
                    CreateAnimationGroup();
                }
            }
        }
    }
}