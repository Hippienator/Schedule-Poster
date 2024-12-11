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
        public static System.Timers.Timer timer;
        public static MainID mainID;

        static async Task Main(string[] args)
        {
            GetIDs();
            SaveLoadHandling.AccountHandling.StartUp();
            await TwitchAPI.GetSchedule(Groups[0].BroadcasterID.ToString(), 5, DateTime.Now);
            await TwitchAPI.GetStream(Groups[0].BroadcasterID.ToString());
            await TwitchAPI.ValidateToken();
            client = new DClient();
            await client.Client.ConnectAsync();
            eventSub = new EventSubWebsocket("wss://eventsub.wss.twitch.tv/ws", TwitchAPI.ClientID, TwitchAPI.AccessToken);
            eventSub.OnConnected += EventSub_OnConnected;
            eventSub.OnStreamOnline += EventSub_OnStreamOnline;
            eventSub.OnStreamOffline += EventSub_OnStreamOffline;
            Thread.Sleep(1000);

            timer = new System.Timers.Timer(UntilMidnight());
            timer.AutoReset = false;
            timer.Elapsed += async (s, e) => await Timer_Elapsed(s,e);
            timer.Start();

            await Task.Delay(-1);
        }

        private static async Task Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
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
                await DoSchedule(group, true);
                await Task.Delay(500);
            }
        }

        public static async Task<bool> DoSchedule(IDGroup group, bool skipCurrent = false)
        {
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
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\MainID.json"))
                {
                    MainID? mainID = JsonConvert.DeserializeObject<MainID>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\MainID.json"));
                    if (mainID != null)
                        Program.mainID = mainID;
                }

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"))
                {
                    IDGroup[]? groups = JsonConvert.DeserializeObject<IDGroup[]>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ID.json"));
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
