using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class RevocationEventArgs : EventArgs
    {
        /// <summary>
        /// The ID of the subscription being revoked
        /// </summary>
        public string SubscriptionID { get; }

        /// <summary>
        /// The reason the subscription was revoked. Options are:
        /// - authorization_revoked — The user in the condition object revoked the authorization that let you get events on their behalf.
        /// - user_removed — The user in the condition object is no longer a Twitch user.
        /// - version_removed — The subscribed to subscription type and version is no longer supported.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Type of the revoked subscription.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Conditions of the revoked subscription.
        /// </summary>
        public Dictionary<string,string> Conditions { get; } = new Dictionary<string,string>();

        public RevocationEventArgs (string subscriptionID, string status, string type, JObject conditions)
        {
            SubscriptionID = subscriptionID;
            Status = status;
            Type = type;
            foreach (KeyValuePair<string, JToken?> token in conditions)
                Conditions.Add(token.Key, token.ToString());
        }
    }
}
