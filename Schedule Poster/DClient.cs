using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Schedule_Poster
{
    internal class DClient
    {
        public DiscordClient Client;

        public DClient()
        {
            DiscordConfiguration config = new DiscordConfiguration() { Token = Program.Token, Intents = DiscordIntents.AllUnprivileged};
            Client = new DiscordClient(config);
            SlashCommandsExtension slash = Client.UseSlashCommands();
            slash.RegisterCommands<SlashCommands>();
        }

        public async Task ModifyMessage(ulong channelID, ulong messageID, string newMessage)
        {
            DiscordChannel channel = await Client.GetChannelAsync(channelID);
            if (channel != null)
            {
                if (messageID != 0)
                {
                    DiscordMessage message = await channel.GetMessageAsync(messageID);
                    if (message != null)
                        await message.ModifyAsync(newMessage);
                    else
                    {
                        message = await channel.SendMessageAsync(newMessage);
                        Program.SetMessageID(message.Id);
                    }
                }
                else
                {
                    DiscordMessage message = await channel.SendMessageAsync(newMessage);
                    Program.SetMessageID(message.Id);
                }
            }
        }
    }
}
