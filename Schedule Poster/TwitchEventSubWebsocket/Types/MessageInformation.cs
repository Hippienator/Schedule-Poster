using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class MessageInformation
    {
        /// <summary>
        /// The text of the entire message.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// An array of the fragments that make up the message.
        /// </summary>
        public MessageFragment[] Fragments { get; }

        public MessageInformation(string text, MessageFragment[] fragments)
        {
            Text = text;
            Fragments = fragments;
        }
    }
}
