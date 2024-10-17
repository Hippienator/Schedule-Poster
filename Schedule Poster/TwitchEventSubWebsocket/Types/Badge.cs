using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class Badge
    {
        /// <summary>
        /// The type of badge, e.g. Bits, Subscriber, World of Warcraft.
        /// </summary>
        public string SetID { get; }

        /// <summary>
        /// The specific badge in the set, for Bits, it would be the tier level. For World of Warcraft it would be Alliance or Horde.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The meta-data of the badge, at time of writing, this only applies to Subscriber badges and contains the months subscribed
        /// </summary>
        public string Info { get; }

        public Badge (string setID, string iD, string info)
        {
            SetID = setID;
            ID = iD;
            Info = info;
        }
    }
}
