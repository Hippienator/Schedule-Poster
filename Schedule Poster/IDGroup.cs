using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    internal class IDGroup
    {
        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong MessageID { get; set; }
        public int BroadcasterID { get; set; }
        public ulong AccouncementID { get; set; }

        public IDGroup()
        {

        }
    }
}
