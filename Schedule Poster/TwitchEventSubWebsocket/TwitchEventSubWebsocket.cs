using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using TwitchEventSubWebsocket.Communication;
using TwitchEventSubWebsocket.Types.Event;
using System.Collections.Generic;
using Timer = System.Timers.Timer;
using TwitchEventSubWebsocket.Types;
using TwitchEventSubWebsocket.SubcriptionHandling;
using Schedule_Poster.Logging;
using System.Net.NetworkInformation;

namespace TwitchEventSubWebsocket
{
    public class EventSubWebsocket
    {
        /// <summary>
        /// The ID of the websocket connection.
        /// </summary>
        public string? SessionID
        {
            get { return sessionID; }
            set { sessionID = value; }
        }

        /// <summary>
        /// Client ID associated with using the Twitch API. Only required if you want this to handle making subscriptions to the EventSub.
        /// </summary>
        public string ClientID { get; set; }

        /// <summary>
        /// The access token for the Twitch API. Only required if you want this to handle making subscriptions to the EventSub.
        /// </summary>
        public string AccessToken;

        /// <summary>
        /// Set of methods to subscribe to different topics on the EventSub.
        /// </summary>
        public SubscriptionHandler Subscribe;

        public string? sessionID;
        private WebsocketClient EventWebsocket;
        private Uri OriginalURI;
        private List<string> MessageIDsOld = new List<string>();
        private List<string> MessageIDsNew = new List<string>();
        private Timer MessageTimer = new Timer() { Interval = 600000, AutoReset = true };
        private Timer ReconnectTimer = new Timer { Interval = 10000, AutoReset = true };
        private Timer KeepAliveTimer = new Timer { Interval = 10000, AutoReset = false };
        private static readonly object MessageListLock = new object();
        private TimeSpan KeepAliveTime = TimeSpan.FromSeconds(10);
        private DateTimeOffset LastMessage;

        #region Events
        /// <summary>
        /// Fires when the websocket disconnect.
        /// </summary>
        public event EventHandler<DisconnectionInfo>? OnDisconnected;

        /// <summary>
        /// Fires when the websocket receives the Welcome message. Remember to add scopes through the API or the connection will close.
        /// </summary>
        public event EventHandler<ConnectedEventArgs>? OnConnected;

        /// <summary>
        /// Fires when a subscription has been revoked.
        /// </summary>
        public event EventHandler<RevocationEventArgs>? OnRevocation;

        /// <summary>
        /// Fires when a user follows a channel that is monitored.
        /// </summary>
        public event EventHandler<ChannelFollowEventArgs>? OnChannelFollow;

        /// <summary>
        /// Fires when when a raid happens, either from a channel monitored for outgoing raids or a channel monitored for incoming raids.
        /// </summary>
        public event EventHandler<RaidEventArgs>? OnRaid;

        /// <summary>
        /// Fires when a monitored channel updates its stream information.
        /// </summary>
        public event EventHandler<ChannelUpdateEventArgs>? OnChannelUpdate;

        /// <summary>
        /// Fires when someone subscribes to a channel being monitored for new subscriptions. Does not fire on resubs.
        /// </summary>
        public event EventHandler<ChannelSubscribeEventArgs>? OnChannelSubscribe;

        /// <summary>
        /// Fires when a chat notification happens. There are a lot of different types of notifications that can fire this.
        /// </summary>
        public event EventHandler<ChatNotificationsEventArgs>? OnChatNotifications;

        /// <summary>
        /// Fires when a subscribed channel starts streaming.
        /// </summary>
        public event EventHandler<StreamOnlineEventArgs>? OnStreamOnline;

        /// <summary>
        /// Fires when a subscribed channel stops streaming.
        /// </summary>
        public event EventHandler<StreamOfflineEventArgs>? OnStreamOffline;

        #endregion

        public EventSubWebsocket(string url, string clientID = "", string accessToken = "")
        {
            Subscribe = new SubscriptionHandler(clientID, accessToken);
            OriginalURI = new Uri(url);
            CreateWebsocket();
            MessageTimer.Elapsed += MessageTimer_Elapsed;
            ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
            ClientID = clientID;
            AccessToken = accessToken;
        }
        private void CreateWebsocket()
        {
            EventWebsocket = new WebsocketClient(OriginalURI);
            EventWebsocket.IsReconnectionEnabled = false;
            EventWebsocket.MessageReceived.Subscribe(m => Task.Run(() => WebsocketMessageHandler(this, m)));
            EventWebsocket.DisconnectionHappened.Subscribe(e => Task.Run(() => WebsocketLostConnection(this, e)));
            Task.Run(() => Connect(OriginalURI));
        }

        public void Dispose()
        {
            EventWebsocket.Dispose();
            MessageTimer.Dispose();
            ReconnectTimer.Dispose();
            KeepAliveTimer.Dispose();
        }

        public void Reconnect()
        {
            try
            {
                EventWebsocket.StopOrFail(WebSocketCloseStatus.EndpointUnavailable, "Went longer than the keepalive time without a message.");
                EventWebsocket.Dispose();
                CreateWebsocket();
            }
            catch (Exception ex)
            {
                Logger.Log($"[Debug] Failed at either stopping or connecting to Eventsocket - {ex.Message}");
            }
        }

        static bool CheckInternetConnectivity()
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 2000); // Google's public DNS
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        #region Timer Events

        //Timer that 
        private void KeepAliveTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTimeOffset.UtcNow - LastMessage > KeepAliveTime)
            {
                Logger.Log($"[Debug]Keep alive timeout. {(DateTimeOffset.UtcNow - LastMessage).Seconds}");
                MessageTimer.Stop();
                Reconnect();
            }
            else
                KeepAliveTimer.Start();
        }

        //Timer to try reconnecting on failed connection.
        private void ReconnectTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (EventWebsocket.IsRunning)
            {
                ReconnectTimer.Stop();
                return;
            }
            EventWebsocket.Start();
        }

        //Handles the removing the storage of message ids to safeguard against replay attacks. The way it's set up message ids stay for 10-20 minutes.
        private void MessageTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            lock (MessageListLock)
            {
                MessageIDsOld = MessageIDsNew;
                MessageIDsNew = new List<string>();
            }
        }
        #endregion

        //Automatically reconnects the websocket if connection is lost.
        private void WebsocketLostConnection(object sender, DisconnectionInfo info)
        {
            if (info?.CloseStatusDescription != null)
                OnDisconnected?.Invoke(this, info);
            MessageTimer.Stop();
            Reconnect();
        }

        //Prepares everything for a fresh connection and connects to the given Uri.
        private void Connect(Uri url)
        {
            EventWebsocket.Url = url;
            MessageIDsOld = new List<string>();
            MessageIDsNew = new List<string>();
            EventWebsocket.Start();
            ReconnectTimer.Start();
        }

        //Handles incoming messages in the websocket
        private void WebsocketMessageHandler(object sender, ResponseMessage e)
        {

            if (e.MessageType != WebSocketMessageType.Text)
            {
                return;
            }

            ServerMessage msg = JsonConvert.DeserializeObject<ServerMessage>(e.Text);


            //This section handles the safeguarding against replay attacks.
            TimeSpan dtime = DateTime.Now - (DateTime)msg?.MetaData["message_timestamp"];
            if (dtime.Minutes >= 10)
                return;
            string messageID = (string)msg.MetaData["message_id"];
            if (MessageIDsNew.Contains(messageID) || MessageIDsOld.Contains(messageID))
                return;
            lock (MessageListLock)
            {
                MessageIDsNew.Add(messageID);
            }

            //This sets the message time for keep alive.
            DateTimeOffset messageTime = (DateTimeOffset)msg?.MetaData["message_timestamp"];
            if (messageTime > LastMessage)
                LastMessage = messageTime;


            switch ((string)msg.MetaData["message_type"])
            {
                //Happens when first connecting
                case "session_welcome":
                    {
                        KeepAliveTimer.Start();
                        KeepAliveTime = TimeSpan.FromSeconds((int)msg.Payload.Session["keepalive_timeout_seconds"] + 5);
                        MessageTimer.Start();
                        string? tempString = (string?)msg.Payload.Session["id"];
                        if (tempString != null)
                        {
                            SessionID = tempString;
                            Subscribe.WebsocketID = SessionID;
                        }
                        OnConnected?.Invoke(this, new ConnectedEventArgs((string)msg.Payload.Session["id"], (int)msg.Payload.Session["keepalive_timeout_seconds"]));
                    }
                    break;

                //Happens when a scope the websocket is subscribed to fires.
                case "notification":
                    {
                        Task.Run(() => NotificationHanlder((string)msg.Payload.Subscription["type"], msg.Payload.Event));
                    }
                    break;

                //Happens when Twitch wants you to reconnect through a new url.
                case "session_reconnect":
                    {
                        EventWebsocket.Url = new Uri((string)msg.Payload.Session["reconnect_url"]);
                        EventWebsocket.StopOrFail(WebSocketCloseStatus.NormalClosure, "Asked to reconnect to differnt URI");
                        EventWebsocket.StartOrFail();
                    }
                    break;

                //Happens when a subscription is revoked.
                case "revocation":
                    {
                        JObject info = msg.Payload.Subscription;
                        OnRevocation?.Invoke(this, new RevocationEventArgs((string)info["id"], (string)info["status"], (string)info["type"], info["conditions"]?.ToObject<JObject>()));
                    }
                    break;

            }
        }

        private void NotificationHanlder(string notifType, JObject args)
        {
            switch (notifType)
            {
                case "channel.follow":
                    {
                        OnChannelFollow?.Invoke(this, new ChannelFollowEventArgs((string)args["user_id"], (string)args["user_login"], (string)args["user_name"],
                            (string)args["broadcaster_user_id"], (string)args["broadcaster_user_login"], (string)args["broadcaster_user_name"], (DateTime)args["followed_at"]));
                        break;
                    }

                case "channel.raid":
                    {
                        OnRaid?.Invoke(this, new RaidEventArgs((string)args["from_broadcaster_user_id"], (string)args["from_broadcaster_user_login"],
                            (string)args["from_broadcaster_user_name"], (string)args["to_broadcaster_user_id"], (string)args["to_broadcaster_user_login"],
                            (string)args["to_broadcaster_user_name"], (int)args["viewers"]));
                        break;
                    }

                case "channel.update":
                    {
                        User broadcaster = new User((string)args["broadcaster_user_id"], (string)args["broadcaster_user_login"], (string)args["broadcaster_user_name"]);
                        OnChannelUpdate?.Invoke(this, new ChannelUpdateEventArgs(broadcaster, (string)args["title"], (string)args["language"], (string)args["category_id"],
                            (string)args["category_name"], args["content_classification_labels"]?.ToObject<string[]>()));
                        break;
                    }

                case "channel.subscribe":
                    {
                        User user = new User((string)args["user_id"], (string)args["user_login"], (string)args["user_name"]);
                        User broadcaster = new User((string)args["broadcaster_user_id"], (string)args["broadcaster_user_login"], (string)args["broadcaster_user_name"]);
                        OnChannelSubscribe?.Invoke(this, new ChannelSubscribeEventArgs(user, broadcaster, (int)args["tier"], (bool)args["is_gift"]));
                        break;
                    }

                case "channel.chat.notification":
                    {
                        ChatNotificationHandler(args);
                        break;
                    }

                case "stream.online":
                    {
                        User broadcaster = new User((string)args["broadcaster_user_id"], (string)args["broadcaster_user_login"], (string)args["broadcaster_user_name"]);
                        OnStreamOnline?.Invoke(this, new StreamOnlineEventArgs((string)args["id"], broadcaster, (string)args["type"], (DateTimeOffset)args["started_at"]));
                        break;
                    }

                case "stream.offline":
                    {
                        User broadcaster = new User((string)args["broadcaster_user_id"], (string)args["broadcaster_user_login"], (string)args["broadcaster_user_name"]);
                        OnStreamOffline?.Invoke(this, new StreamOfflineEventArgs(broadcaster));
                        break;
                    }
            }
        }


        private void ChatNotificationHandler(JObject args)
        {
            User broadcaster = new User((string)args["broadcaster_user_id"], (string)args["broadcaster_user_name"], (string)args["broadcaster_user_login"]);
            User chatter = new User((string)args["chatter_user_id"], (string)args["chatter_user_name"], (string)args["chatter_user_login"]);
            bool isChatterAnon = (bool)args["chatter_is_anonymous"];
            string color = (string)args["color"];
            Badge[] badges = BadgesHandler((JArray)args["badges"]);
            string systemMessage = (string)args["system_message"];
            string messageID = (string)args["message_id"];
            MessageInformation message = MessageBuilder((JObject)args["message"]);
            string noticeType = (string)args["notice_type"];
            
            switch(noticeType)
            {
                case "sub":
                    {
                        JObject subInfo = (JObject)args["sub"];
                        if (subInfo != null)
                        {
                            SubscriptionNotification sub = new SubscriptionNotification((int)subInfo["sub_tier"], (bool)subInfo["is_prime"], (int)subInfo["duration_months"]);
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, sub);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "resub":
                    {
                        JObject resubInfo = (JObject)args["resub"];
                        if (resubInfo != null)
                        {
                            int cumulative = (int)args["cumulative_months"];
                            int duration = (int)args["duration_months"];
                            int streak = (int)args["streak_months"];
                            int tier = (int)args["sub_tier"];
                            bool isPrime = (bool)args["is_prime"];
                            bool isGift = (bool)resubInfo["is_gift"];
                            if (isGift)
                            {
                                bool gifterIsAnon = (bool)args["gifter_is_anonymous"];
                                if (gifterIsAnon)
                                {
                                    ResubscriptionNotification resub = new ResubscriptionNotification(cumulative, duration, streak, tier, isPrime, isGift, gifterIsAnon);
                                    ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                        messageID, message, noticeType, resub: resub);
                                    OnChatNotifications?.Invoke(this, eventArgs);
                                }
                                else
                                {
                                    User user = new User((string)args["gifter_user_id"], (string)args["gifter_user_name"], (string)args["gifter_user_login"]);
                                    ResubscriptionNotification resub = new ResubscriptionNotification(cumulative, duration, streak, tier, isPrime, isGift, gifterIsAnon, user);
                                    ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                        messageID, message, noticeType, resub: resub);
                                    OnChatNotifications?.Invoke(this, eventArgs);
                                }

                            }
                            else
                            {
                                ResubscriptionNotification resub = new ResubscriptionNotification(cumulative, duration, streak, tier, isPrime);
                                ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                    messageID, message, noticeType, resub: resub);
                                OnChatNotifications?.Invoke(this, eventArgs);
                            }
                        }
                        break;
                    }

                case "sub_gift":
                    {
                        JObject subGiftInfo = (JObject)args["sub_gift"];
                        if (subGiftInfo != null)
                        {
                            int duration = (int)subGiftInfo["duration_months"];
                            int cumulative = (int)subGiftInfo["cumulative_total"];
                            User recipient = new User((string)subGiftInfo["recipient_user_id"], (string)subGiftInfo["recipient_user_name"], (string)subGiftInfo["recipient_user_login"]);
                            int tier = (int)subGiftInfo["sub_tier"];
                            string communityGiftID = (string)subGiftInfo["community_gift_id"];
                            SubscriptionGiftNotification subGift = new SubscriptionGiftNotification(duration, recipient, tier, cumulative, communityGiftID);
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, subGift: subGift);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "community_sub_gift":
                    {
                        JObject comGiftInfo = (JObject)args["community_sub_gift"];
                        if (comGiftInfo != null)
                        {
                            string id = (string)comGiftInfo["id"];
                            int total = (int)comGiftInfo["total"];
                            int tier = (int)comGiftInfo["sub_tier"];
                            int cumulative = (int)comGiftInfo["cumulative_total"];
                            CommunitySubGiftNotification comSub = new CommunitySubGiftNotification(id, total, tier, cumulative);
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, communitySubGift: comSub);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "gift_paid_upgrade":
                    {
                        JObject giftPaidInfo = (JObject)args["gift_paid_upgrade"];
                        if (giftPaidInfo != null)
                        {
                            bool gifterIsAnon = (bool)giftPaidInfo["gifter_is_anonymous"];
                            if (gifterIsAnon)
                            {
                                GiftPaidUpgradeNotification giftPaid = new GiftPaidUpgradeNotification(gifterIsAnon);
                                ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                    messageID, message, noticeType, giftPaidUpgrade: giftPaid);
                                OnChatNotifications?.Invoke(this, eventArgs);
                            }
                            else
                            {
                                User gifter = new User((string)giftPaidInfo["gifter_user_id"], (string)giftPaidInfo["gifter_user_name"], (string)giftPaidInfo["gifter_user_login"]);
                                GiftPaidUpgradeNotification giftPaid = new GiftPaidUpgradeNotification(gifterIsAnon, gifter);
                                ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                    messageID, message, noticeType, giftPaidUpgrade: giftPaid);
                                OnChatNotifications?.Invoke(this, eventArgs);
                            }
                        }
                        break;
                    }

                case "prime_paid_upgrade":
                    {
                        JObject primePaidInfo = (JObject)args["prime_paid_upgrade"];
                        if (primePaidInfo != null)
                        {
                            PrimeUpgradeNotification prime = new PrimeUpgradeNotification((int)primePaidInfo["sub_tier"]);
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, primePaidUpgrade: prime);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "raid":
                    {
                        JObject raidInfo = (JObject)args["raid"];
                        if (raidInfo != null)
                        {
                            User raider = new User((string)raidInfo["user_id"], (string)raidInfo["user_name"], (string)raidInfo["user_login"]);
                            int count = (int)raidInfo["viewer_count"];
                            string imageURL = (string)raidInfo["profile_image_url"];
                            RaidNotification raid = new RaidNotification(raider, count, imageURL);
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, raid: raid);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "unraid":
                    {
                        ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                            messageID, message, noticeType);
                        OnChatNotifications?.Invoke(this, eventArgs);
                        break;
                    }

                case "pay_it_foward":
                    {
                        JObject payItInfo = (JObject)args["pay_it_forward"];
                        if (payItInfo != null)
                        {
                            bool isGifterAnon = (bool)payItInfo["gifter_is_anonymous"];
                            if (isGifterAnon)
                            {
                                ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                    messageID, message, noticeType, payItForward: new PayItForwardNotification(isGifterAnon));
                                OnChatNotifications?.Invoke(this, eventArgs);
                            }
                            else
                            {
                                User gifter = new User((string)payItInfo["gifter_user_id"], (string)payItInfo["gifter_user_name"], (string)payItInfo["gifter_user_login"]);
                                ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                    messageID, message, noticeType, payItForward: new PayItForwardNotification(isGifterAnon, gifter));
                                OnChatNotifications?.Invoke(this, eventArgs);
                            }
                        }
                        break;
                    }

                case "announcement":
                    {
                        JObject annoucementInfo = (JObject)args["announcement"];
                        if (annoucementInfo != null)
                        {
                            AnnouncementNotification announcement = new AnnouncementNotification((string)annoucementInfo["color"]);
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, announcement: announcement);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "bits_badge_tier":
                    {
                        JObject bitsBadgeInfo = (JObject)args["bits_badge_tier"];
                        if (bitsBadgeInfo != null)
                        {
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, bitsBadge: new BitsBadgeNotification((int)bitsBadgeInfo["tier"]));
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }

                case "charity_donation":
                    {
                        JObject charityInfo = (JObject)args["charity_donation"];
                        if (charityInfo != null)
                        {
                            string charityName = (string)charityInfo["charity_name"];
                            JObject amountInfo = (JObject)charityInfo["amount"];
                            CharityNotfication charity = new CharityNotfication(charityName, new Amount((int)amountInfo?["value"], (int)amountInfo?["decimal_place"], (string)amountInfo?["currency"]));
                            ChatNotificationsEventArgs eventArgs = new ChatNotificationsEventArgs(broadcaster, chatter, isChatterAnon, color, badges, systemMessage,
                                messageID, message, noticeType, charityDonation: charity);
                            OnChatNotifications?.Invoke(this, eventArgs);
                        }
                        break;
                    }
            }
        }

        private Badge[] BadgesHandler(JArray args)
        {
            List<Badge> badges = new List<Badge>();
            foreach (JObject badge in args)
                badges.Add(new Badge((string)badge["set_id"], (string)badge["id"], (string)badge["info"]));

            return badges.ToArray();
        }

        private MessageInformation MessageBuilder(JObject args)
        {
            string text = (string)args["text"];
            JArray fragmentArray = (JArray)args["fragments"];
            List<MessageFragment> fragments = new List<MessageFragment>();

            if (fragmentArray != null)
            {
                foreach (JObject fragment in fragmentArray)
                {
                    string type = (string)fragment["type"];
                    string fragtext = (string)fragment["text"];
                    switch (type)
                    {
                        case "cheeremote":
                            {
                                JObject cheerEmoteInfo = (JObject)fragment["cheeremote"];
                                CheerEmote cheerEmote = new CheerEmote((string)cheerEmoteInfo?["prefix"], (int)cheerEmoteInfo?["bits"], (int)cheerEmoteInfo?["tier"]);
                                fragments.Add(new MessageFragment(type, fragtext, cheerEmote));
                                break;
                            }

                        case "emote":
                            {
                                JObject emoteInfo = (JObject)fragment["emote"];
                                JArray jArray = (JArray)emoteInfo?["format"];
                                List<string> formats = new List<string>();
                                if (jArray != null)
                                {
                                    foreach (string format in jArray)
                                        formats.Add(format);
                                }
                                Emote emote = new Emote((string)emoteInfo?["id"], (string)emoteInfo?["emote_set_id"], (string)emoteInfo?["owner_id"], formats.ToArray());
                                fragments.Add(new MessageFragment(type, fragtext, emote: emote));
                                break;
                            }

                        case "mention":
                            {
                                JObject mentionInfo = (JObject)fragment["mention"];
                                User user = new User((string)mentionInfo?["user_id"], (string)mentionInfo?["user_name"], (string)mentionInfo?["user_login"]);
                                fragments.Add(new MessageFragment(type, fragtext, mention: new Mention(user)));
                                break;
                            }

                        case "text":
                            {
                                fragments.Add(new MessageFragment(type, fragtext));
                                break;
                            }
                    }
                }

            }

            return new MessageInformation(text, fragments.ToArray());
        }
    }
}