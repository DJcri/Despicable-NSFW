using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public class Settings : ModSettings
    {
        // Player settings
        public bool facialPartsExtensionEnabled = true;
        public bool lovinExtensionEnabled = true;
        public bool heroModuleEnabled = true;
        public bool debugMode = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref facialPartsExtensionEnabled, "facialPartsExtensionEnabled", true);
            Scribe_Values.Look(ref lovinExtensionEnabled, "lovinExtensionEnabled", true);
            Scribe_Values.Look(ref heroModuleEnabled, "heroModuleEnabled", true);
            Scribe_Values.Look(ref debugMode, "debugMode", false);
            base.ExposeData();
        }
    }
}
