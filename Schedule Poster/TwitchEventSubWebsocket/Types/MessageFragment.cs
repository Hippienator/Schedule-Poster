using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class MessageFragment
    {
        /// <summary>
        /// The type of fragment, options are text, cheeremote, emote, mention.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The text of the message fragment
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Information on the cheeremote. If type isn't cheeremote, this will be null.
        /// </summary>
        public CheerEmote? CheerEmote { get; }

        /// <summary>
        /// Information on the emote. If type isn't emote, this will be null.
        /// </summary>
        public Emote? Emote { get; }

        /// <summary>
        /// Information on the mention. If type isn't mention, this will be null.
        /// </summary>
        public Mention? Mention { get; }

        public MessageFragment (string type, string text, CheerEmote? cheerEmote = null, Emote? emote = null, Mention? mention = null)
        {
            Type = type;
            Text = text;
            CheerEmote = cheerEmote;
            Emote = emote;
            Mention = mention;
        }
    }
}
