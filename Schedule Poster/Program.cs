using DSharpPlus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.RateLimiting;
using System.Xml;
using TwitchEventSubWebsocket;
using Schedule_Poster.Logging;

namespace Schedule_Poster
{
    internal class Program
    {
        public static string Token = "";
        public static List<IDGroup> Groups = new List<IDGroup>();
        public static readonly object saveLock = new object();
        public static EventSubWebsocket eventSub;
        public static DClient client;
        public static System.Timers.Timer timer;
        public static MainID mainID;

        static async Task Main(string[] args)
        {
            Logger.Initialize();
            TwitchAPI.lastRenewed = DateTime.Now.AddMinutes(-2);
            GetIDs();
            SaveLoadHandling.AccountHandling.StartUp();
            Logger.Log("[Starting]IDs and credentials loaded.");

            HttpResponseMessage validation = await TwitchAPI.ValidateToken();
            if (validation.IsSuccessStatusCode)
                Logger.Log("[Starting]Accesstoken validated");
            else
            {
                Logger.Log("[Starting][Critical]Accesstoken not working. Renewing");
                HttpStatusCode statusCode = await TwitchAPI.RenewToken();
                if (statusCode == HttpStatusCode.OK)
                    Logger.Log("[Starting]Successfully renewed");
                else
                {
                    Logger.Log("[Starting][Critical]Failed renewal");
                    System.Environment.Exit(0);
                }
            }

            Logger.Log("[Starting]Starting Discord client.");
            client = new DClient();
            await client.Client.ConnectAsync();

            Logger.Log("[Starting]Starting EventSocket.");
            eventSub = new EventSubWebsocket("wss://eventsub.wss.twitch.tv/ws", TwitchAPI.ClientID, TwitchAPI.AccessToken);
            eventSub.OnConnected += EventSub_OnConnected;
            eventSub.OnDisconnected += EventSub_OnDisconnected;
            eventSub.OnStreamOnline += EventSub_OnStreamOnline;
            eventSub.OnStreamOffline += EventSub_OnStreamOffline;

            timer = new System.Timers.Timer(UntilMidnight());
            timer.AutoReset = false;
            timer.Elapsed += async (s, e) => await Timer_Elapsed(s,e);
            timer.Start();

            await Task.Delay(-1);
        }

        private static void EventSub_OnDisconnected(object? sender, Websocket.Client.DisconnectionInfo e)
        {
            Logger.Log($"[Critical]Lost connection to EventSub. Reason: {e.CloseStatusDescription}");
        }

        private static async Task Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Logger.Log("[Info]Running midnight scheduled updates.");
            await DoAllSubscribed();
            timer.Interval = UntilMidnight().TotalMilliseconds;
            timer.Start();
        }

        public static TimeSpan UntilMidnight()
        {

            DateTime tomorrow = DateTime.UtcNow.AddDays(1);
            DateTime nextMidnight = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day);
            TimeSpan diff = nextMidnight - DateTime.UtcNow;
            return diff;
        }

        public static async Task DoAllSubscribed()
        {
            foreach (IDGroup group in Groups)
            {
                bool success = await DoSchedule(group, true);
                if (success)
                    Logger.Log($"[Info]Successfully updated schedule for {group.BroadcasterID}");
                else
                    Logger.Log($"[Critical]Failed to update schedule for {group.BroadcasterID}");
                await Task.Delay(500);
            }
        }

        public static async Task<bool> DoSchedule(IDGroup group, bool skipCurrent = false)
        {
            Logger.Log($"[Info]Attempting to update schedule for {group.BroadcasterID}");
            if (group.ChannelID == 0 || group.BroadcasterID == 0)
                return false;
            System.Threading.RateLimiting.RateLimitLease lease = await TwitchAPI.rateLimiter.AcquireAsync();
            if (!lease.IsAcquired)
                return false;
            StreamInformation? streamInformation = await TwitchAPI.GetStream(group.BroadcasterID.ToString());
            lease = await TwitchAPI.rateLimiter.AcquireAsync();
            if (!lease.IsAcquired)
                return false;
            List<ScheduleInformation> streams = await TwitchAPI.GetSchedule(group.BroadcasterID.ToString(), group.NumberOfStreams, DateTime.UtcNow, streamInformation, skipCurrent); //Hipbotnator: "764108031"
            if (streams.Count == 0) 
                return false;
            string toSend = "";
            for (int i = 0; i < streams.Count; i++)
            {
                toSend += streams[i].GameName + Environment.NewLine + streams[i].TimeCode;
                if (i != streams.Count - 1)
                    toSend += Environment.NewLine + "--------------------------------------" + Environment.NewLine;
            }

            await client.ModifyMessage(group, toSend);
            return true;
        }

        private static async void EventSub_OnStreamOffline(object? sender, TwitchEventSubWebsocket.Types.Event.StreamOfflineEventArgs e)
        {
            Logger.Log($"[Info]Streamer {e.Broadcaster.Displayname} started streaming.");
            IDGroup? group = Groups.Find(x => x.BroadcasterID.ToString() == e.Broadcaster.ID);
            if (group != null)
                await DoSchedule(group, true);
        }

        private static async void EventSub_OnStreamOnline(object? sender, TwitchEventSubWebsocket.Types.Event.StreamOnlineEventArgs e)
        {
            Logger.Log($"[Info]Streamer {e.Broadcaster.Displayname} stopped streaming.");
            IDGroup? group = Groups.Find(x => x.BroadcasterID.ToString() == e.Broadcaster.ID);
            if (group != null)
                await DoSchedule(group);
        }

        private static async void EventSub_OnConnected(object? sender, TwitchEventSubWebsocket.Types.Event.ConnectedEventArgs e)
        {
            Logger.Log($"[Info]Connected to Eventsub");
            foreach (IDGroup group in Groups)
            {
                Logger.Log($"[Info]Subscribing to watch {group.BroadcasterID}");
                RateLimitLease lease = await TwitchAPI.rateLimiter.AcquireAsync(1);
                if (lease.IsAcquired)
                    eventSub.Subscribe.SubscribeToStreamOnline(group.BroadcasterID.ToString());
                lease = await TwitchAPI.rateLimiter.AcquireAsync(1);
                if (lease.IsAcquired)
                    eventSub.Subscribe.SubscribeToStreamOffline(group.BroadcasterID.ToString());
            }
        }

        public static void SetMessageID(ulong messageID, IDGroup group)
        {
            if (group != null)
            {
                group.MessageID = messageID;
                SaveIDs();
            }
        }

        public static void SaveIDs()
        {
            lock (saveLock)
            {
                Logger.Log("[Info]Saving IDs");
                using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "ID.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    IDGroup[] groups = Groups.ToArray();
                    serializer.Serialize(file, groups);
                }
            }
        }

        public static void GetIDs()
        {
            lock (saveLock) 
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "MainID.json"))
                {
                    MainID? mainID = JsonConvert.DeserializeObject<MainID>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MainID.json"));
                    if (mainID != null)
                        Program.mainID = mainID;
                }

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ID.json"))
                {
                    IDGroup[]? groups = JsonConvert.DeserializeObject<IDGroup[]>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "ID.json"));
                    if (groups != null)
                    {
                        foreach (IDGroup group in groups)
                        {
                            Groups.Add(group);
                        }
                    }
                }
            }
        }
    }
}
