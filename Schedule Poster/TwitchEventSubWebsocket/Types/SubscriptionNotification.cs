using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class SubscriptionNotification
    {
        /// <summary>
        /// Tier of subscription
        /// </summary>
        public int Tier { get; }

        /// <summary>
        /// Whether the subscription was a prime subscription. All prime subs have a Tier value of 1.
        /// </summary>
        public bool IsPrime { get; }

        /// <summary>
        /// The amount of months just subscribed for. This will be 1, unless it's a multi-month sub.
        /// </summary>
        public int Duration { get; }

        public SubscriptionNotification(int tier, bool isPrime, int duration)
        {
            Tier = tier / 1000;
            IsPrime = isPrime;
            Duration = duration;
        }
    }
}
