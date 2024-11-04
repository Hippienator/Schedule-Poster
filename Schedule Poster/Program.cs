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
        public static List<IDGroup> Groups = new List<IDGroup>();
        public static readonly object saveLock = new object();
        public static EventSubWebsocket eventSub;
        public static DClient client;

        static async Task Main(string[] args)
        {
            SaveLoadHandling.AccountHandling.StartUp();
            GetIDs();
            await TwitchAPI.ValidateToken();
            client = new DClient();
            await client.Client.ConnectAsync();
            eventSub = new EventSubWebsocket("wss://eventsub.wss.twitch.tv/ws", TwitchAPI.ClientID, TwitchAPI.AccessToken);
            eventSub.OnConnected += EventSub_OnConnected;
            eventSub.OnStreamOnline += EventSub_OnStreamOnline;
            eventSub.OnStreamOffline += EventSub_OnStreamOffline;
            Thread.Sleep(1000);

            foreach (IDGroup group in Groups)
            {
                await DoSchedule(group, true);
            }

            await Task.Delay(-1);
        }

        public static async Task DoSchedule(IDGroup group, bool skipCurrent = false)
        {
            StreamInformation? streamInformation = await TwitchAPI.GetStream(group.BroadcasterID.ToString());
            List<ScheduleInformation> streams = await TwitchAPI.GetSchedule(group.BroadcasterID.ToString(), 5, DateTime.UtcNow, streamInformation, skipCurrent); //Hipbotnator: "764108031"
            string toSend = "";
            for (int i = 0; i < streams.Count; i++)
            {
                toSend += streams[i].GameName + Environment.NewLine + streams[i].TimeCode;
                if (i != streams.Count - 1)
                    toSend += Environment.NewLine + "--------------------------------------" + Environment.NewLine;
            }

            await client.ModifyMessage(group, toSend);
        }

        private static async void EventSub_OnStreamOffline(object? sender, TwitchEventSubWebsocket.Types.Event.StreamOfflineEventArgs e)
        {
            IDGroup? group = Groups.Find(x => x.BroadcasterID.ToString() == e.Broadcaster.ID);
            if (group != null)
                await DoSchedule(group, true);
        }

        private static async void EventSub_OnStreamOnline(object? sender, TwitchEventSubWebsocket.Types.Event.StreamOnlineEventArgs e)
        {
            IDGroup? group = Groups.Find(x => x.BroadcasterID.ToString() == e.Broadcaster.ID);
            if (group != null)
                await DoSchedule(group);
        }

        private static void EventSub_OnConnected(object? sender, TwitchEventSubWebsocket.Types.Event.ConnectedEventArgs e)
        {
            foreach (IDGroup group in Groups)
            {
                eventSub.Subscribe.SubscribeToStreamOnline(group.BroadcasterID.ToString());
                eventSub.Subscribe.SubscribeToStreamOffline(group.BroadcasterID.ToString());
            }
        }

        public static void SetMessageID(ulong messageID, IDGroup group)
        {
            if (group != null)
            {
                lock (saveLock)
                {
                    group.MessageID = messageID;
                    using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        IDGroup[] groups = Groups.ToArray();
                        serializer.Serialize(file, groups);
                    }
                }
            }
        }

        public static void GetIDs()
        {
            lock (saveLock) 
            {
                //JObject json = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"));
                IDGroup[]? groups = JsonConvert.DeserializeObject<IDGroup[]>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"));
                if (groups != null)
                {
                    foreach(IDGroup group in groups) 
                    {
                        Groups.Add(group);
                    }
                }
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
