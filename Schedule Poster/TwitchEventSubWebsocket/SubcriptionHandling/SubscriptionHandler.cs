using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Schedule_Poster;


namespace TwitchEventSubWebsocket.SubcriptionHandling
{
    public class SubscriptionHandler
    {
        private string ClientID;
        private string AccessToken;
        public string? WebsocketID;

        public SubscriptionHandler(string clientID, string accessToken)
        {
            ClientID = clientID;
            AccessToken = accessToken;
        }

        public void UpdateToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        private async Task<bool> Subscribe(string paramters, bool TwitchCLI)
        {
            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Client-ID", ClientID);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string setSubscriptionUrl = "";

                if (TwitchCLI)
                    setSubscriptionUrl = "http://127.0.0.1:8080/eventsub/subscriptions";
                else
                    setSubscriptionUrl = "https://api.twitch.tv/helix/eventsub/subscriptions";

                var response = await client.PostAsync(setSubscriptionUrl, new StringContent(paramters, Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
        }

        public bool SubscribeToChannelRaid(string toBroadcasterID = "", string fromBroadcasterID = "", bool TwitchCLI = false)
        {
            SubParameters json = new SubParameters();
            json.type = "channel.raid";
            json.version = "1";
            json.transport.Add("session_id", WebsocketID);
            if (toBroadcasterID != "")
                json.condition.Add("to_broadcaster_user_id", toBroadcasterID);
            else if (fromBroadcasterID != "")
                json.condition.Add("from_broadcaster_user_id", fromBroadcasterID);
            else
                return false;


            string parameters = JsonConvert.SerializeObject(json);
            return Subscribe(parameters, TwitchCLI).Result;
        }

        public bool SubscribeToChatNotification(string broadcasterID, string userID, bool TwitchCLI = false)
        {
            //Currently not implemented in the CLI.
            if (TwitchCLI)
                return false;

            SubParameters json = new SubParameters();
            json.type = "channel.chat.notification";
            json.version = "1";
            json.transport.Add("session_id", WebsocketID);
            json.condition.Add("broadcaster_user_id", broadcasterID);
            json.condition.Add("user_id", userID);


            string parameters = JsonConvert.SerializeObject(json);
            return Subscribe(parameters, TwitchCLI).Result;
        }

        public bool SubscribeToStreamOnline(string broadcasterID, bool TwitchCLI = false)
        {
            SubParameters json = new SubParameters();
            json.type = "stream.online";
            json.version = "1";
            json.transport.Add("session_id", WebsocketID);
            json.condition.Add("broadcaster_user_id", broadcasterID);

            string parameters = JsonConvert.SerializeObject(json);
            return Subscribe(parameters, TwitchCLI).Result;
        }

        public bool SubscribeToStreamOffline(string broadcasterID, bool TwitchCLI = false)
        {
            SubParameters json = new SubParameters();
            json.type = "stream.offline";
            json.version = "1";
            json.transport.Add("session_id", WebsocketID);
            json.condition.Add("broadcaster_user_id", broadcasterID);

            string parameters = JsonConvert.SerializeObject(json);
            return Subscribe(parameters, TwitchCLI).Result;
        }
        public bool SubscribeToChannelFollow(string broadcasterID, bool TwitchCLI = false)
        {
            SubParameters json = new SubParameters();
            json.type = "channel.follow";
            json.version = "2";
            json.transport.Add("session_id", WebsocketID);
            json.condition.Add("broadcaster_user_id", broadcasterID);
            json.condition.Add("moderator_user_id", broadcasterID);


            string parameters = JsonConvert.SerializeObject(json);
            return Subscribe(parameters, TwitchCLI).Result;
        }
    }
}
