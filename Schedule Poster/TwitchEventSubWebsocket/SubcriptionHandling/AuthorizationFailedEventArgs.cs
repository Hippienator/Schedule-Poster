using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.SubcriptionHandling
{
    public class AuthorizationFailedEventArgs
    {
        /// <summary>
        /// The Parameters of the subscription attempt where autherization failed. Can be sent back with the Subscribe method.
        /// </summary>
        public string Parameters { get; }

        /// <summary>
        /// Whether the subscription was sent to the TwitchCLI or not. Can be sent back with the Subscribe method.
        /// </summary>
        public bool TwitchCLI { get; }

        public AuthorizationFailedEventArgs(string parameters, bool twitchCLI)
        {
            Parameters = parameters;
            TwitchCLI = twitchCLI;
        }
    }
}
