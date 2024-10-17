using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class ConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The websocket connection ID.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The maximum amount of time between messages.
        /// </summary>
        public int KeepaliveTime { get; }

        public ConnectedEventArgs (string id, int keepaliveTime)
        {
            ID = id;
            KeepaliveTime = keepaliveTime;
        }

    }
}
