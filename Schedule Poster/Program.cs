using DSharpPlus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using TwitchEventSubWebsocket;

namespace Schedule_Poster
{
    internal class Program
    {
        public static string Token = "";
        public static IDGroup Group = new IDGroup();
        public static readonly object saveLock = new object();
        public static EventSubWebsocket eventSub;
        public static DClient client;

        static async Task Main(string[] args)
        {
            GetIDs();
            GetCredentials();
            //Group.BroadcasterID = 764108031;
            client = new DClient();
            await client.Client.ConnectAsync();
            eventSub = new EventSubWebsocket("wss://eventsub.wss.twitch.tv/ws", TwitchAPI.ClientID, TwitchAPI.AccessToken);
            eventSub.OnConnected += EventSub_OnConnected;
            eventSub.OnStreamOnline += EventSub_OnStreamOnline;
            eventSub.OnStreamOffline += EventSub_OnStreamOffline;
            Thread.Sleep(1000);

            await DoSchedule(true);
            
            await Task.Delay(-1);
        }

        public static async Task DoSchedule(bool skipCurrent = false)
        {
            StreamInformation? streamInformation = await TwitchAPI.GetStream(Group.BroadcasterID.ToString());
            List<ScheduleInformation> streams = await TwitchAPI.GetSchedule(Group.BroadcasterID.ToString(), 5, DateTime.UtcNow, streamInformation, skipCurrent); //Hipbotnator: "764108031"
            string toSend = "";
            for (int i = 0; i < streams.Count; i++)
            {
                toSend += streams[i].GameName + Environment.NewLine + streams[i].TimeCode;
                if (i != streams.Count - 1)
                    toSend += Environment.NewLine + "--------------------------------------" + Environment.NewLine;
            }

            await client.ModifyMessage(Group.ChannelID, Group.MessageID, toSend);
        }

        private static async void EventSub_OnStreamOffline(object? sender, TwitchEventSubWebsocket.Types.Event.StreamOfflineEventArgs e)
        {
            await DoSchedule(true);
        }

        private static async void EventSub_OnStreamOnline(object? sender, TwitchEventSubWebsocket.Types.Event.StreamOnlineEventArgs e)
        {
            await DoSchedule();
        }

        private static void EventSub_OnConnected(object? sender, TwitchEventSubWebsocket.Types.Event.ConnectedEventArgs e)
        {
            eventSub.Subscribe.SubscribeToStreamOnline(Group.BroadcasterID.ToString());
            eventSub.Subscribe.SubscribeToStreamOffline(Group.BroadcasterID.ToString());
        }

        public static void SetMessageID(ulong messageID)
        {
            if (Group != null)
            {
                lock (saveLock)
                {
                    Group.MessageID = messageID;
                    using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //serialize object directly into file stream
                        serializer.Serialize(file, Group);
                    }
                }
            }
        }

        public static void GetIDs()
        {
            lock (saveLock) 
            {
                //JObject json = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"));
                IDGroup? group = JsonConvert.DeserializeObject<IDGroup>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"));
                if (group != null)
                    Group = group;
            }
        }

        public static void GetCredentials()
        {
            Credentials? cred = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Cred.json"));
            if (cred != null)
            {
                Token = cred.Token;
                TwitchAPI.ClientID = cred.ClientID;
                TwitchAPI.AccessToken = cred.AccessToken;
            }
        }
    }
}
