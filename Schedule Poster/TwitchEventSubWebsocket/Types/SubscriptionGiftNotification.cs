using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class SubscriptionGiftNotification
    {
        /// <summary>
        /// Months of subscription that was gifted.
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// The total amount of subs that have been gifted by the gifter. If gifter is anonymous this will be null.
        /// </summary>
        public int? CumulativeGiven { get; }

        /// <summary>
        /// The user who received the gift sub.
        /// </summary>
        public User Recipient { get; }

        /// <summary>
        /// The tier of the gifted sub.
        /// </summary>
        public int Tier { get; }

        /// <summary>
        /// The ID of the community gift. If it was not a community gift this is null.
        /// </summary>
        public string? CommunityGiftID { get; }
        public SubscriptionGiftNotification(int duration, User recipient, int tier,  int? cumulativeGiven = null, string? communityGiftID = null)
        {
            Duration = duration;
            CumulativeGiven = cumulativeGiven;
            Recipient = recipient;
            Tier = tier / 1000;
            CommunityGiftID = communityGiftID;
        }
    }
}
