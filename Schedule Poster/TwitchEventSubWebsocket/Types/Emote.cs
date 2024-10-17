using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class Emote
    {
        /// <summary>
        /// The uniquie ID for the emote.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The ID of the set the emote belongs to.
        /// </summary>
        public string EmoteSetID { get; }

        /// <summary>
        /// The ID of the broadcaster the emote is from.
        /// </summary>
        public string OwnerID { get; }

        /// <summary>
        /// Can contain: static, animated or both.
        /// </summary>
        public string[] Format {  get; }

        public Emote (string iD, string emoteSetID, string ownerID, string[] format)
        {
            ID = iD;
            EmoteSetID = emoteSetID;
            OwnerID = ownerID;
            Format = format;
        }
    }
}
