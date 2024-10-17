using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Communication
{
    internal class ServerMessage
    {
        [JsonProperty(PropertyName = "metadata")]
        public JObject MetaData { set; get; }

        [JsonProperty(PropertyName = "payload")]
        public Payload Payload { get; set; }
    }

    internal class Payload
    {
        [JsonProperty(PropertyName = "session")]
        public JObject Session { set; get; }

        [JsonProperty(PropertyName = "subscription")]
        public JObject Subscription { set; get; }

        [JsonProperty(PropertyName = "event")]
        public JObject Event { set; get; }
    }
}
