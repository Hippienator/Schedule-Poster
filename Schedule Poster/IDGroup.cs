using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    internal class IDGroup
    {
        //Discord IDs
        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong MessageID { get; set; }
        public ulong AccouncementID { get; set; }

        public int BroadcasterID { get; set; }
        public int NumberOfStreams { get; set; }

        //Variables for doing pings when the schedule streamer goes online
        public bool OnlinePingEnabled { get; set; }
        public ulong? OnlinePingChannelID { get; set; }
        public string? OnlinePingMessage { get; set; }
        public ulong? OnlinePingRoleID { get; set; }

        public IDGroup(ulong guildID)
        {
            GuildID = guildID;
            NumberOfStreams = 5;
            OnlinePingEnabled = false;
        }
    }
}
