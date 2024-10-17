using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class User
    {
        /// <summary>
        /// The given user's ID
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The given user's login name (same as channelname)
        /// </summary>
        public string Login { get; }

        /// <summary>
        /// The given user's displayname.
        /// </summary>
        public string Displayname { get; }

        public User (string id, string login, string displayname)
        {
            ID = id;
            Login = login;
            Displayname = displayname;
        }
    }
}
