using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class Reward
    {
        /// <summary>
        /// The ID of the reward.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The title of the reward.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The channel point cost of the reward.
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// The description of the reward.
        /// </summary>
        public string Prompt { get; set; }
        
        public Reward(string id, string title, int cost, string prompt)
        {
            ID = id;
            Title = title;
            Cost = cost;
            Prompt = prompt;
        }
    }
}
