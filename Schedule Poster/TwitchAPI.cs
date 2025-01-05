using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

namespace Schedule_Poster
{
    internal static class TwitchAPI
    {
        public static string ClientID = string.Empty;
        public static string AccessToken = string.Empty;
        public static string RefreshToken = string.Empty;
        public static string ClientSecret = string.Empty;
        private static readonly object renewLock = new object();
        public static DateTime lastRenewed;
        private static bool currentlyRenewing = false;
        public static System.Threading.RateLimiting.RateLimiter rateLimiter = new System.Threading.RateLimiting.TokenBucketRateLimiter(new System.Threading.RateLimiting.TokenBucketRateLimiterOptions() 
        { TokenLimit = 800, AutoReplenishment = true, ReplenishmentPeriod = TimeSpan.FromSeconds(3), TokensPerPeriod = 20, QueueLimit = 800, QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst });

        public static async Task<List<ScheduleInformation>> GetSchedule(string channelID, int results, DateTime timeRequested, StreamInformation? streamInformation = null, bool skipCurrent = false)
        {
            List<ScheduleInformation> streams = new List<ScheduleInformation>();
            if (skipCurrent)
                results++;
            bool skipFirst = false;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Client-ID", ClientID);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

                string getScheduleUrl = $"https://api.twitch.tv/helix/schedule?broadcaster_id={channelID}&first={results}&start_time={XmlConvert.ToString(timeRequested, XmlDateTimeSerializationMode.Utc)}";
                HttpResponseMessage scheduleResponse = await client.GetAsync(getScheduleUrl);
                if (scheduleResponse.IsSuccessStatusCode)
                {
                    string scheduleJson = await scheduleResponse.Content.ReadAsStringAsync();
                    JObject scheduleResult = JObject.Parse(scheduleJson);
                    if (scheduleResult != null)
                    {
                        JObject? data = (JObject?)scheduleResult["data"];
                        if (data != null)
                        {
                            JArray? jArray = (JArray?)data["segments"];

                            if (jArray != null)
                            {
                                for (int i = 0; i < jArray.Count; i++)
                                {
                                    JObject? stream = (JObject?)jArray[i];
                                    string timeCode = "";
                                    DateTimeOffset time = (DateTimeOffset)stream["start_time"];
                                    if (i == 0)
                                    {
                                        if (streamInformation != null)
                                        {
                                            TimeSpan timeDifference = time - streamInformation.StartTime;
                                            if (timeDifference.Hours >= 6)
                                            {
                                                streams.Add(new ScheduleInformation($"Live since <t:{streamInformation.StartTime.ToUnixTimeSeconds()}:R>", "Unscheduled stream"));
                                                timeCode = $"<t:{time.ToUnixTimeSeconds()}:F>";
                                                string? cancelled = (string?)stream["canceled_until"];
                                                if (cancelled != null)
                                                    timeCode = "~~" + timeCode + "~~ (Cancelled)";
                                            }
                                            else
                                                timeCode = $"<t:{time.ToUnixTimeSeconds()}:F> - Live since <t:{streamInformation.StartTime.ToUnixTimeSeconds()}:R>";
                                        }
                                        else if (skipCurrent && time < DateTimeOffset.UtcNow)
                                        {
                                            skipFirst = true;
                                        }
                                        else
                                        {
                                            timeCode = $"<t:{time.ToUnixTimeSeconds()}:F>";
                                            string? cancelled = (string?)stream["canceled_until"];
                                            if (cancelled != null)
                                                timeCode = "~~" + timeCode + "~~ (Cancelled)";
                                        }
                                    }
                                    else
                                    {
                                        timeCode = $"<t:{time.ToUnixTimeSeconds()}:F>";
                                        string? cancelled = (string?)stream["canceled_until"];
                                        if (cancelled != null)
                                            timeCode = "~~" + timeCode + "~~ (Cancelled)";
                                    }
                                    string gameName = "Game to be announced";
                                    if (stream["category"]?.HasValues ?? false)
                                    {
                                        JObject? game = (JObject?)stream["category"];
                                        gameName = (string?)game?["name"] ?? "Game to be announced";
                                    }
                                    streams.Add(new ScheduleInformation(timeCode, gameName));
                                }
                            }
                        }
                    }

                    if (skipCurrent)
                    {
                        if (skipFirst)
                            streams.Remove(streams[0]);
                        else
                            streams.Remove(streams[streams.Count - 1]);
                    }

                    if (streamInformation != null && streams.Count > results)
                        streams.Remove(streams[streams.Count - 1]);
                }
                else
                {
                    HttpStatusCode code = await RenewToken();
                    if (code == HttpStatusCode.OK)
                        return await GetSchedule(channelID, results, timeRequested, streamInformation, skipCurrent);
                }
            }
            return streams;
        }

        public static async Task<HttpResponseMessage> ValidateToken()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"OAuth {AccessToken}");
                HttpResponseMessage response = await client.GetAsync("https://id.twitch.tv/oauth2/validate");

                return response;
            }
        }

        public static async Task<int?> GetUserID(string name)
        {
            System.Threading.RateLimiting.RateLimitLease lease = await rateLimiter.AcquireAsync();
            if (!lease.IsAcquired)
                return null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Client-ID", ClientID);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");
                HttpResponseMessage response = await client.GetAsync($"https://api.twitch.tv/helix/users?login={name}");
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject jObject = JObject.Parse(content);
                    JArray? data = (JArray?)jObject["data"];
                    JObject? dataObject = (JObject?)data?[0];
                    int? id = (int?)dataObject?["id"];

                    return id;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpStatusCode renew = await RenewToken();
                    if (renew == HttpStatusCode.OK)
                        return await GetUserID(name);
                }
                return null;
            }
        }

        public static async Task<StreamInformation?> GetStream(string channelID)
        {
            System.Threading.RateLimiting.RateLimitLease lease = await rateLimiter.AcquireAsync();
            if (!lease.IsAcquired)
                return null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Client-ID", ClientID);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

                string getStreamUrl = $"https://api.twitch.tv/helix/streams?user_id={channelID}";
                HttpResponseMessage streamResponse = await client.GetAsync(getStreamUrl);
                if (streamResponse.IsSuccessStatusCode)
                {
                    string streamJson = await streamResponse.Content.ReadAsStringAsync();
                    JObject streamResult = JObject.Parse(streamJson);

                    JArray? data = (JArray?)streamResult["data"];
                    JObject? jObject = null;
                    if (data.Count > 0)
                        jObject = (JObject?)data[0];
                    if (jObject != null)
                    {
                        DateTimeOffset time = (DateTimeOffset)jObject["started_at"];
                        string game = (string)jObject["game_name"];
                        return new StreamInformation(time, game);
                    }
                    return null;
                }
                else if (streamResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpStatusCode renew = await RenewToken();
                    if (renew == HttpStatusCode.OK)
                        return await GetStream(channelID); 
                }
            }
            return null;
        }

        public static async Task<HttpStatusCode> RenewToken()
        {
            lock (renewLock)
            {
                if (currentlyRenewing)
                    return HttpStatusCode.TooManyRequests;
                TimeSpan sinceLastRenew = DateTime.Now - lastRenewed;
                if (sinceLastRenew.TotalSeconds < 60)
                    return HttpStatusCode.TooManyRequests;
                currentlyRenewing = true;
            }

            HttpStatusCode code = HttpStatusCode.RequestTimeout;

            using (HttpClient client = new HttpClient())
            {
                FormUrlEncodedContent request = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id",ClientID),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", RefreshToken)
                });
                HttpResponseMessage response = await client.PostAsync("https://id.twitch.tv/oauth2/token", request);
                code = response.StatusCode;
                string test = await response.Content.ReadAsStringAsync();
                if (code == HttpStatusCode.OK)
                {
                    JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                    string? access = (string?)result["access_token"];
                    if (access != null)
                        AccessToken = access;
                    string? refresh = (string?)result["refresh_token"];
                    if (refresh != null)
                        RefreshToken = refresh;
                    SaveLoadHandling.AccountHandling.SaveTokens();
                    if (Program.eventSub?.Subscribe != null)
                        Program.eventSub.Subscribe.UpdateToken(AccessToken);
                }
                lastRenewed = DateTime.Now;
                currentlyRenewing = false;
            }
            return code;
        }
    }
}
