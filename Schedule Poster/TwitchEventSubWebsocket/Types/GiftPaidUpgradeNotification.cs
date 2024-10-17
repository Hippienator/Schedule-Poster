using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class GiftPaidUpgradeNotification
    {
        /// <summary>
        /// Is the original gifter of the sub getting upgraded anonymous?
        /// </summary>
        public bool IsGifterAnonymous { get; set; }

        /// <summary>
        /// The user who originally gifted a sub.
        /// </summary>
        public User? Gifter { get; }

        public GiftPaidUpgradeNotification (bool isGifterAnonymous, User? gifter = null)
        {
            IsGifterAnonymous = isGifterAnonymous;
            Gifter = gifter;
        }
    }
}
