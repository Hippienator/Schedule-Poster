using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class PrimeUpgradeNotification
    {
        /// <summary>
        /// The tier being upgraded to.
        /// </summary>
        public int Tier { get; }

        public PrimeUpgradeNotification(int tier)
        {
            Tier = tier / 1000;
        }
    }
}
