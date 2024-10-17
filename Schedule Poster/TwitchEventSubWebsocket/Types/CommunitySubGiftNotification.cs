using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class CommunitySubGiftNotification
    {
        /// <summary>
        /// The ID for this batch of community gift subs.
        /// </summary>
        public string CommunityGiftID { get; }

        /// <summary>
        /// The total amount of gifts in this batch of community gift subs
        /// </summary>
        public int TotalGifts { get; }

        /// <summary>
        /// The tier of the community gift subs in this batch.
        /// </summary>
        public int Tier {  get; }

        /// <summary>
        /// The total amount of subs the gifter has gifted. If the gifter is anonymous this is null.
        /// </summary>
        public int? CumulativeTotal { get; }

        public CommunitySubGiftNotification(string communityGiftID, int totalGifts, int tier, int? cumulativeTotal = null)
        {
            CommunityGiftID = communityGiftID;
            TotalGifts = totalGifts;
            Tier = tier/1000;
            CumulativeTotal = cumulativeTotal;
        }
    }
}
