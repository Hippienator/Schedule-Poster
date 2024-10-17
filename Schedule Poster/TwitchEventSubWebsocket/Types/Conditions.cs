using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipbotnator.TwitchEventSubWebsocket.Types
{
    public class Conditions
    {
        public string BroadcasterUserID { get; } = null;
        public string ModeratorUserID { get; } = null;
        public string UserID { get; } = null;
        public string FromBroadcasterUserID { get; } = null;
        public string ToBroadcasterUserID { get; } = null;
        public string RewardID { get; } = null;
        public string ClientID { get; } = null;
        public string ConduitID { get; } = null;
        public string OrganizationID { get; } = null;
        public string CategoryID { get; } = null;
        public string CampaignID { get; } = null;
        public string ExtensionClientID { get; } = null;

        public Conditions(JObject conditions)
        {
            BroadcasterUserID = (string)conditions["broadcaster_user_id"];
            ModeratorUserID = (string)conditions["moderator_user_id"];
            UserID = (string)conditions["user_id"];
            FromBroadcasterUserID = (string)conditions["from_broadcaster_user_id"];
            ToBroadcasterUserID = (string)conditions["to_broadcaster_user_id"];
            RewardID = (string)conditions["reward_id"];
            ClientID = (string)conditions["client_id"];
            ConduitID = (string)conditions["conduit_id"];
            OrganizationID = (string)conditions["organization_id"];
            CategoryID = (string)conditions["category_id"];
            CampaignID = (string)conditions["campaign_id"];
            ExtensionClientID = (string)conditions["extension_client_id"];
        }
    }
}
