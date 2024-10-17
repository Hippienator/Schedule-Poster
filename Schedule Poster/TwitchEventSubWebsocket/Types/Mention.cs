using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class Mention
    {
        /// <summary>
        /// The user being mentioned.
        /// </summary>
        public User User { get; }

        public Mention(User user)
        {
            User = user;
        }
    }
}
