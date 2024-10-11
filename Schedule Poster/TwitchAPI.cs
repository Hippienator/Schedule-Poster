using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    internal static class TwitchAPI
    {
        public static string ClientID = " ";
        public static string AccessToken = " ";
        public static async Task<List<ScheduleInformation>> GetSchedule(string channelID, int results)
        {
            List<ScheduleInformation> streams = new List<ScheduleInformation>();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Client-ID", ClientID);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

                string getScheduleUrl = $"https://api.twitch.tv/helix/schedule?broadcaster_id={channelID}&first={results}";
                HttpResponseMessage scheduleResponse = await client.GetAsync(getScheduleUrl);
                string scheduleJson = await scheduleResponse.Content.ReadAsStringAsync();
                JObject scheduleResult = JObject.Parse(scheduleJson);
                if (scheduleResult != null)
                {
                    JObject data = (JObject)scheduleResult["data"];
                    if (data != null)
                    {
                        JArray jArray = (JArray)data["segments"];

                        foreach (JObject stream in jArray)
                        {
                            DateTimeOffset time = (DateTimeOffset)stream["start_time"];
                            string timeCode =$"<t:{time.ToUnixTimeSeconds().ToString()}:F>";
                            string gameName = "Game to be announced";
                            if (stream["category"].HasValues)
                            {
                                JObject game = (JObject)stream["category"];
                                gameName = (string)game["name"];
                            }
                            streams.Add(new ScheduleInformation(timeCode, gameName));
                        }
                    }
                }
            }
            return streams;
        }
    }
}
