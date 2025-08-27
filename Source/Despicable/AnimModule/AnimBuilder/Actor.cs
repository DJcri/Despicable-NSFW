using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class Actor
    {
        public Actor(string roleName, int gender)
        {
            this.roleName = roleName;
            this.gender = gender;
        }

        // Keyframe parts
        public PawnRenderNodeTagDef nodeSelected;
        public int angle = 0;
        public string angleBuffer;
        public bool visible = true;
        public Rot4 rotation = Rot4.South;
        public Vector3 offset = Vector3.zero;
        public string soundDef = "Sound_Def";

        // Animation parts
        public string animName;
        public List<ExtendedKeyframe> extendedKeyframes;
        public Dictionary<PawnRenderNodeTagDef, KeyframeAnimationPart> keyframeParts;

        // Role parts
        public string roleName;
        public int gender = 0;
        public List<AnimationDef> roleAnimations;

        public void OnNewStage()
        {
            extendedKeyframes.Clear();
            keyframeParts.Clear();
        }

        public void Reset()
        {
            nodeSelected = null;
            angle = 0;
            angleBuffer = "0";
            visible = true;
            rotation = Rot4.South;
            offset = Vector3.zero;
            soundDef = "";
            animName = null;
            roleName = "role name";
            gender = 0;
            if (!roleAnimations.NullOrEmpty())
                roleAnimations.Clear();
            if (!extendedKeyframes.NullOrEmpty())
                extendedKeyframes.Clear();
            if (!keyframeParts.NullOrEmpty())
                keyframeParts.Clear();
        }
    }
}
