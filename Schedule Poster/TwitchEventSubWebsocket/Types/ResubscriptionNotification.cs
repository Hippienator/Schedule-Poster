using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class ResubscriptionNotification
    {
        /// <summary>
        /// The total amount of months the subscriber has subscribed for.
        /// </summary>
        public int CumulativeMonths { get; }

        /// <summary>
        /// The amount of months just subscribed for. This will be 1, unless it's a multi-month sub.
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// The amount of months in a row the subscriber has been subbed for. This is optional in the response.
        /// </summary>
        public int? Streak { get; }

        /// <summary>
        /// Tier of subscription
        /// </summary>
        public int Tier { get; }

        /// <summary>
        /// Is the subscription made a prime subscription? If this is true Tier will be 1.
        /// </summary>
        public bool IsPrime { get; }

        /// <summary>
        /// Is this a gift sub?
        /// </summary>
        public bool IsGift { get; }

        /// <summary>
        /// Was this subscription gifted anonymously? Is null if IsGift is false.
        /// </summary>
        public bool? IsGifterAnon { get; }

        /// <summary>
        /// The user that gifted the subscription. Is null if IsGift or IsGifterAnon are false.
        /// </summary>
        public User? Gifter { get; }

        public ResubscriptionNotification(int cumulativeMonths, int duration, int? streak, int tier, bool isPrime, bool isGift = false, bool? isGifterAnon = null, User? gifter = null)
        {
            CumulativeMonths = cumulativeMonths;
            Duration = duration;
            Streak = streak;
            Tier = tier / 1000;
            IsPrime = isPrime;
            IsGift = isGift;
            IsGifterAnon = isGifterAnon;
            Gifter = gifter;
        }
    }
}
