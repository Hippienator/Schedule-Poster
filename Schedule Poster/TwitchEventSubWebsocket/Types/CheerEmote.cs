using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class CheerEmote
    {
        /// <summary>
        /// The text part of the cheeremote, if Cheer100 was used to donate 100 bits, Prefix will be Cheer.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The amount of bits cheered.
        /// </summary>
        public int Bits { get; }

        /// <summary>
        /// The tier of cheeremote shown.
        /// </summary>
        public int Tier { get; }

        public CheerEmote(string prefix, int bits, int tier)
        {
            Prefix = prefix;
            Bits = bits;
            Tier = tier;
        }
    }
}
