using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class ChannelFollowEventArgs : EventArgs
    {
        /// <summary>
        /// User who followed.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Broadcaster that got the follow.
        /// </summary>
        public User Broadcaster { get; }

        /// <summary>
        /// The time the follow happened.
        /// </summary>
        public DateTime FollowedAt { get; }

        public ChannelFollowEventArgs(string userID, string userLogin, string userDisplayname, string broadcasterID, string broadcasterLogin, 
            string broadcasterDisplayname, DateTime followedAt)
        {
            User = new User(userID, userLogin, userDisplayname);
            Broadcaster = new User(broadcasterID, broadcasterLogin, broadcasterDisplayname);
            FollowedAt = followedAt;
        }
    }
}
