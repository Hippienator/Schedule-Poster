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
            slash.RegisterCommands<AdminCommands>(Program.mainID.GuildID);

        }

        public async Task ModifyMessage(IDGroup group, string newMessage)
        {
            DiscordChannel channel = await Client.GetChannelAsync(group.ChannelID);
            if (channel != null)
            {
                if (group.MessageID != 0)
                {
                    DiscordMessage message = await channel.GetMessageAsync(group.MessageID);
                    if (message != null)
                        await message.ModifyAsync(newMessage);
                    else
                    {
                        message = await channel.SendMessageAsync(newMessage);
                        Program.SetMessageID(message.Id, group);
                    }
                }
                else
                {
                    DiscordMessage message = await channel.SendMessageAsync(newMessage);
                    Program.SetMessageID(message.Id, group);
                }
            }
        }
    }
}
