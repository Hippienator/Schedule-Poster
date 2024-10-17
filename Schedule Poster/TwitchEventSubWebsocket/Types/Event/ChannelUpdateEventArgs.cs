using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class ChannelUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The broadcaster who's channel updated.
        /// </summary>
        public User Broadcaster { get; }

        /// <summary>
        /// Title of the stream.
        /// </summary>
        public string? Title { get; }

        /// <summary>
        /// The language the stream is set to.
        /// </summary>
        public string? Language { get; }

        /// <summary>
        /// The ID of the category of the stream.
        /// </summary>
        public string? CategoryID { get; }

        /// <summary>
        /// The name of the catergory of the stream.
        /// </summary>
        public string? CategoryName { get; }

        /// <summary>
        /// An array of the labels of the content classification.
        /// </summary>
        public string[]? ContentClassifcations { get; }

        public ChannelUpdateEventArgs (User broadcaster, string title, string language, string categoryID, string categoryName, string[] contentClassifications)
        {
            Broadcaster = broadcaster;
            Title = title;
            Language = language;
            CategoryID = categoryID;
            CategoryName = categoryName;
            ContentClassifcations = contentClassifications;
        }
    }
}
