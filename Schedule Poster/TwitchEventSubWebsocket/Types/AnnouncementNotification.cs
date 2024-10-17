using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class AnnouncementNotification
    {
        /// <summary>
        /// Color of the announcement.
        /// </summary>
        public string Color { get; }

        public AnnouncementNotification(string color)
        {
            Color = color;
        }
    }
}
