using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class RaidNotification
    {
        /// <summary>
        /// The user raiding the channel.
        /// </summary>
        public User Raider { get; }

        /// <summary>
        /// The number of raiders in the raid.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The url to the profile image of the raiding channel.
        /// </summary>
        public string ProfileImage { get; }

        public RaidNotification(User raider, int count, string profileImage)
        {
            Raider = raider;
            Count = count;
            ProfileImage = profileImage;
        }
    }
}
