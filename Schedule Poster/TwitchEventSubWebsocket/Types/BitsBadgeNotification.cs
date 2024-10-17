using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class BitsBadgeNotification
    {
        /// <summary>
        /// The tier of the bits badge earned. The tier value is equal to the amount of bits needed to earn it.
        /// </summary>
        public int Tier { get; }

        public BitsBadgeNotification(int tier)
        {
            Tier = tier;
        }
    }
}
