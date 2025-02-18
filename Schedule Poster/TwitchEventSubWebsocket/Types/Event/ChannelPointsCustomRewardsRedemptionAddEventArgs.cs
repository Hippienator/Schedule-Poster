using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class ChannelPointsCustomRewardsRedemptionAddEventArgs
    {
        /// <summary>
        /// The ID of the redemption made by the user.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The broadcaster who had a redemption added to their reward queue.
        /// </summary>
        public User Broadcaster { get; set; }

        /// <summary>
        /// The user that did the redemption.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The input provided to the redeem, if possible. Empty string if not provided.
        /// </summary>
        public string UserInput { get; set; }

        /// <summary>
        /// The status of the redeem. Options are: unknown, unfulfilled(default), fulfilled, canceled.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The reward tbat was redeened.
        /// </summary>
        public Reward Reward { get; set; }

        /// <summary>
        /// The timestamp of when the redeem was made.
        /// </summary>
        public DateTime RedeemedAt { get; set; }

        public ChannelPointsCustomRewardsRedemptionAddEventArgs(string iD, User broadcaster, User user, string userInput, string status, Reward reward, DateTime redeemedAt)
        {
            ID = iD;
            Broadcaster = broadcaster;
            User = user;
            UserInput = userInput;
            Status = status;
            Reward = reward;
            RedeemedAt = redeemedAt;
        }
    }
}
