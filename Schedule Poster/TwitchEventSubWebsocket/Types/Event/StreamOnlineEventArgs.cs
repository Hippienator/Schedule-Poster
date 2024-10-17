using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types.Event
{
    public class StreamOnlineEventArgs
    {
        public enum StreamType
        {
            Live,
            Playlist,
            WatchParty,
            Premiere,
            Rerun
        }

        /// <summary>
        /// The ID of the stream.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Information on the broadcaster of the stream.
        /// </summary>
        public User Broadcaster { get; }

        /// <summary>
        /// Information on the type of stream. Can be: Live, Playlist, WatchParty, Premiere or Rerun.
        /// </summary>
        public StreamType Type { get; }

        /// <summary>
        /// When the stream started.
        /// </summary>
        public DateTimeOffset StartedAt { get; }

        public StreamOnlineEventArgs(string iD, User broadcaster, string type, DateTimeOffset startedAt)
        {
            ID = iD;
            Broadcaster = broadcaster;
            switch (type)
            {
                case "live":
                    Type = StreamType.Live;
                    break;

                case "playlist":
                    Type = StreamType.Playlist;
                    break;

                case "watch_party":
                    Type = StreamType.WatchParty;
                    break;

                case "premiere":
                    Type = StreamType.Premiere;
                    break;

                case "rerun":
                    Type = StreamType.Rerun;
                    break;
            }
            StartedAt = startedAt;
        }
    }
}
