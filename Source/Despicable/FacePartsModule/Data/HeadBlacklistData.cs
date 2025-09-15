using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Despicable
{
    // A serializable class to represent the XML data structure.
    public class HeadBlacklistData
    {
        [XmlArray("blacklistedHeads")] // Defines the XML array name
        [XmlArrayItem("li")]          // Defines the name of each list item
        public List<string> blacklistedHeadNames = new List<string>();
    }
}
