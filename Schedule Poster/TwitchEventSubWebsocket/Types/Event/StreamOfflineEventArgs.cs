using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class StreamOfflineEventArgs
    {
        public User Broadcaster { get; }
        public StreamOfflineEventArgs(User broadcaster)
        {
            Broadcaster = broadcaster;
        }
    }
}
