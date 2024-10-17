using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class RaidEventArgs : EventArgs
    {
        /// <summary>
        /// The broadcaster that initiated the raid.
        /// </summary>
        public User Raider { get; }

        /// <summary>
        /// The broadcaster that received the raid.
        /// </summary>
        public User Broadcaster { get; }

        /// <summary>
        /// The number of people in the raid.
        /// </summary>
        public int RaiderCount { get; }

        public RaidEventArgs(string raiderID, string raiderLogin, string raiderDisplayname, string broadcasterID, string broadcasterLogin, string broadcasterDisplayname, int raiderCount)
        {
            Raider = new User(raiderID, raiderLogin, raiderDisplayname);
            Broadcaster = new User(broadcasterID, broadcasterLogin, broadcasterDisplayname);
            RaiderCount = raiderCount;
        }
    }
}
