using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Schedule_Poster
{
    internal class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("UpdateSchedule","Updates the schedule.", false)]
        public async Task DoTask(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
                return;
            await Program.DoSchedule(group);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Schedule updated."));
        }

        [SlashCommand("SkipCurrent", "Updates the schedule with the currently scheduled stream skipped.", false)]
        public async Task DoTaskSkip(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
                return;
            await Program.DoSchedule(group, true);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Schedule updated, with the currently scheduled stream skipped."));
        }

        [SlashCommand("SetScheduleChannel", "Sets this channel as the one to do the schedule in.", false)]
        public async Task SetScheduleChannel(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
                group = new IDGroup(ctx.Guild.Id);
            group.ChannelID = ctx.Channel.Id;
            group.MessageID = 0;
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Channel {ctx.Channel.Mention} has been selected as the channel to post the schedule in."));
        }

        [SlashCommand("SetAnnouncementChannel", "Sets this channel as the channel used to post announcements about the bot in.", false)]
        public async Task SetAnnouncementChannel(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
                group = new IDGroup(ctx.Guild.Id);
            group.AccouncementID = ctx.Channel.Id;
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Channel {ctx.Channel.Mention} has been selected as the channel to post announcements from the bot in."));
        }

        [SlashCommand("ShownStreams", "Sets the amount of streams to be shown from the schedule.")]
        public async Task SetNumberOfStreams(InteractionContext ctx, [Option("number","The number of streams to be shown from the schedule. Maximum of 25.")] long number)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
                group = new IDGroup(ctx.Guild.Id);
            if (number < 1)
                group.NumberOfStreams = 1;
            else if (number > 25)
                group.NumberOfStreams = 25;
            else
                group.NumberOfStreams = (int)number;

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The number of streams to be shown has been set to {group.NumberOfStreams}"));
        }

        [SlashCommand("SetStreamerTwitch", "")]
    }
}
