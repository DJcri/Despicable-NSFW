using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;

namespace Despicable
{
    public class HeroGizmo : Command
    {
        private Pawn pawn;
        public override string Desc => "Set hero or see karma";
        public override string Label => "HeroGizmo".Translate();

        public HeroGizmo(Pawn pawn)
        {
            icon = TexIcons.HeroGizmo;
            this.pawn = pawn;
        }

        public override void ProcessInput(Event ev)
        {
            // Open hero screen
            if (ev.button == ((int)MouseButton.LeftMouse))
            {
                CompHero heroComp = pawn.TryGetComp<CompHero>();
                /*
                if (CommonUtil.GetSettings().debugMode)
                    Find.WindowStack.Add(new Dialog_AnimBuilder());
                else
                */
                    Find.WindowStack.Add(new Dialog_HeroScreen(pawn));
            }
        }
    }
}
