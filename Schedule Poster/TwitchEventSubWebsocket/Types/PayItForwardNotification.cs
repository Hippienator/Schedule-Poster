using TwitchEventSubWebsocket.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class PayItForwardNotification
    {

        public bool IsGifterAnon { get; }
        public User? Gifter {  get; }

        public PayItForwardNotification(bool isGifterAnon, User? gifter = null)
        {
            IsGifterAnon = isGifterAnon;
            Gifter = gifter;
        }
    }
}
