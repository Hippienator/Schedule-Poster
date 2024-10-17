using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class ChatNotificationsEventArgs : EventArgs
    {
        /// <summary>
        /// The broadcaster whose channel has the chat notification.
        /// </summary>
        public User Broadcaster { get; }

        /// <summary>
        /// The person triggering the chat notification.
        /// </summary>
        public User Chatter { get; }

        /// <summary>
        /// Whether the person triggering the notifcation is anonymous.
        /// </summary>
        public bool IsChatterAnon { get; }

        /// <summary>
        /// The color of the user that triggered the notification.
        /// </summary>
        public string Color { get; }

        /// <summary>
        /// Array of the badges the user triggering the notification is showing.
        /// </summary>
        public Badge[] Badges { get; }

        /// <summary>
        /// The message that Twitch sends in chat to show the notification.
        /// </summary>
        public string SystemMessage { get; }

        /// <summary>
        /// The UUID of the message.
        /// </summary>
        public string MessageID { get; }

        /// <summary>
        /// The message sent with the notification.
        /// </summary>
        public MessageInformation Message { get; }

        /// <summary>
        /// The type of notification. Options are: sub, resub, sub_gift, community_sub_gift, gift_paid_upgrade, prime_paid_upgrade, raid, unraid,
        /// pay_it_forward, announcement, bits_badge_tier, charity_donation.
        /// </summary>
        public string NoticeType { get; }

        /// <summary>
        /// Information from first time subscribers.
        /// </summary>
        public SubscriptionNotification? Sub { get; }

        /// <summary>
        /// Information on resubscriptions
        /// </summary>
        public ResubscriptionNotification? Resub { get; }

        /// <summary>
        /// Information on gifted subscriptions
        /// </summary>
        public SubscriptionGiftNotification? SubGift { get; }

        /// <summary>
        /// Information on the entire batch of subscriptions gifted to the community.
        /// </summary>
        public CommunitySubGiftNotification? CommunitySubGift { get; }

        /// <summary>
        /// Information on when someone upgrades a gift sub to a paid sub (can be a prime sub).
        /// </summary>
        public GiftPaidUpgradeNotification? GiftPaidUpgrade { get; }

        /// <summary>
        /// Information on when someone upgrades a prime sub to a paid sub.
        /// </summary>
        public PrimeUpgradeNotification? PrimePaidUpgrade { get; }

        /// <summary>
        /// Information on an incoming raid.
        /// </summary>
        public RaidNotification? Raid { get; }

        /// <summary>
        /// Information on someone gifting a sub after receiving a gifted sub.
        /// </summary>
        public PayItForwardNotification? PayItForward { get; }

        /// <summary>
        /// Information on when an announcement is made.
        /// </summary>
        public AnnouncementNotification? Announcement { get; }

        /// <summary>
        /// Information on when someone makes a donation to charity.
        /// </summary>
        public CharityNotfication? CharityDonation { get; }

        /// <summary>
        /// Information on when someone earns a new bits badge.
        /// </summary>
        public BitsBadgeNotification? BitsBadge { get; }

        public ChatNotificationsEventArgs(User broadcaster, User chatter, bool isChatterAnon, string color, Badge[] badges,
            string systemMessage, string messageID, MessageInformation message, string noticeType, SubscriptionNotification? sub = null,
            ResubscriptionNotification? resub = null, SubscriptionGiftNotification? subGift = null,
            CommunitySubGiftNotification? communitySubGift = null, GiftPaidUpgradeNotification? giftPaidUpgrade = null,
            PrimeUpgradeNotification? primePaidUpgrade = null, RaidNotification? raid = null, PayItForwardNotification? payItForward = null,
            AnnouncementNotification? announcement = null, CharityNotfication? charityDonation = null, BitsBadgeNotification? bitsBadge = null)
        {
            Broadcaster = broadcaster;
            Chatter = chatter;
            IsChatterAnon = isChatterAnon;
            Color = color;
            Badges = badges;
            SystemMessage = systemMessage;
            MessageID = messageID;
            Message = message;
            NoticeType = noticeType;
            Sub = sub;
            Resub = resub;
            SubGift = subGift;
            CommunitySubGift = communitySubGift;
            GiftPaidUpgrade = giftPaidUpgrade;
            PrimePaidUpgrade = primePaidUpgrade;
            Raid = raid;
            PayItForward = payItForward;
            Announcement = announcement;
            CharityDonation = charityDonation;
            BitsBadge = bitsBadge;
        }
    }
}
