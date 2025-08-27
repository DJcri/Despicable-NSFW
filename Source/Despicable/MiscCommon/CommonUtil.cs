using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Despicable
{
    [StaticConstructorOnStartup]
    public static class CommonUtil
    {
        // [General Utilities]
        public static Settings GetSettings()
        {
            return LoadedModManager.GetMod<Despicable>().GetSettings<Settings>();
        }

        public static void DebugLog(string msg)
        {
            if (GetSettings().debugMode)
                Log.Message(msg);
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
