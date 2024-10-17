using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    internal class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("UpdateSchedule","Updates the schedule.", false)]
        public async Task DoTask(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            await Program.DoSchedule();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Schedule updated."));
        }

        [SlashCommand("SkipCurrent", "Updates the schedule with the currently scheduled stream skipped.", false)]
        public async Task DoTaskSkip(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            await Program.DoSchedule(true);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Schedule updated, with the currently scheduled stream skipped."));
        }
    }
}
