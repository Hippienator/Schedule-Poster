using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.SubcriptionHandling
{
    internal class SubParameters
    {
        public string type;
        public string version;
        public Dictionary<string, string> condition;
        public Dictionary<string, string> transport;

        public SubParameters()
        {
            condition = new Dictionary<string, string>();
            transport = new Dictionary<string, string>() { { "method", "websocket" } };
        }
    }
}
