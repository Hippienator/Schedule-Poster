using DSharpPlus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Schedule_Poster
{
    internal class Program
    {
        public static string Token = "";
        public static IDGroup Group = new IDGroup();
        public static readonly object saveLock = new object();

        static async Task Main(string[] args)
        {
            GetIDs();
            GetCredentials();
            DClient client = new DClient();
            Thread.Sleep(1000);
            List<ScheduleInformation> streams = await TwitchAPI.GetSchedule(Group.BroadcasterID.ToString(), 5);
            string toSend = "";
            for (int i = 0; i < streams.Count; i++) 
            {
                toSend += streams[i].GameName + Environment.NewLine + streams[i].TimeCode;
                if (i != streams.Count -1)
                    toSend += Environment.NewLine + "--------------------------------------" + Environment.NewLine;
            }

            await client.ModifyMessage(Group.ChannelID, Group.MessageID, toSend);
            await Task.Delay(-1);
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
