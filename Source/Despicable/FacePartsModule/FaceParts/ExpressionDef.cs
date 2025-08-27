using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class ExpressionDef : Def
    {
        public string texPathEyes;
        public string texPathMouth;
        public string texPathDetail;
        public Vector3? eyesOffset;
        public Vector3? mouthOffset;
        public Vector3? detailOffset;

        public Vector3? getOffset(string facePartLabel)
        {
            Vector3? offset = null;

            switch (facePartLabel)
            {
                case "FacePart_Eye_L":
                case "FacePart_Eye_R":
                    offset = eyesOffset;
                    break;
                case "FacePart_Mouth_L":
                case "FacePart_Mouth_R":
                    offset = mouthOffset;
                    break;
                case "FacePart_SecondaryDetail_L":
                case "FacePart_SecondaryDetail_R":
                    offset = detailOffset;
                    break;
            }

            return offset;
        }
    }
}
