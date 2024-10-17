using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class ChannelSubscribeEventArgs : EventArgs
    {
        /// <summary>
        /// User that subscribed.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// The broadcaster that got a subscriber.
        /// </summary>
        public User Broadcaster { get; }

        /// <summary>
        /// Tier of subscription. 1 includes both tier 1 and prime subs.
        /// </summary>
        public int Tier { get; }

        /// <summary>
        /// Whether the subscription was gifted.
        /// </summary>
        public bool IsGift { get; }

        public ChannelSubscribeEventArgs(User user, User broadcaster, int tier, bool isGift)
        {
            User = user;
            Broadcaster = broadcaster;
            Tier = tier/1000;
            IsGift = isGift;
        }
    }
}
