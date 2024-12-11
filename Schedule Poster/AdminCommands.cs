using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    internal class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("SendAnnouncement", "Sends and announcement to all channels that have set up an announcement channel.", false)]
        public async Task SendAnnouncement(InteractionContext ctx, [Option("text", "Text to send in the announcement")] string txt)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            int i = 0;
            foreach (IDGroup group in Program.Groups)
            {
                if (group.AccouncementID != 0)
                {
                    DiscordChannel channel = await Program.client.Client.GetChannelAsync(group.ChannelID);
                    if ( channel != null)
                    {
                        await channel.SendMessageAsync(txt);
                        i++;
                    }
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Announcement posted in {i} out of {Program.Groups.Count} servers."));
        }
    }
}
