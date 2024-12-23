using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Schedule_Poster
{
    internal class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("BugReport", "Sends a bug report.", false)]
        public async Task BugReport(InteractionContext ctx, [Option("contact-allowed", "Sets whether I'm allowed to contact you for further information. True for yes, False for no.")] bool contactAllowed, [Option("text", "The text of the bug report")] string text)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            DiscordChannel channel = await Program.client.Client.GetChannelAsync(Program.mainID.BugChannelID);
            DiscordUser user = await Program.client.Client.GetUserAsync(Program.mainID.AdminID);
            if (channel != null)
            {
                string reply = "Thank you for your report, it has been successfully sent.";
                if (contactAllowed)
                {
                    text += $" - Contact: {ctx.User.Username}";
                    if (user != null)
                        reply += $" You might be contacted by the user {user.Username} for further information.";
                }
                await channel.SendMessageAsync(text);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(reply));
            }
        }

        [SlashCommand("UpdateSchedule","Updates the schedule.", false)]
        public async Task DoTask(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This Discord server is not setup."));
                return;
            }
            bool success = await Program.DoSchedule(group);
            
            if (success)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Schedule updated."));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to update schedule."));
        }

        [SlashCommand("SkipCurrent", "Updates the schedule with the currently scheduled stream skipped.", false)]
        public async Task DoTaskSkip(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This Discord server is not setup."));
                return;
            }
            bool success = await Program.DoSchedule(group, true);

            if (success)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Schedule updated, with the first currently scheduled stream skipped."));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to update schedule."));
        }

        [SlashCommand("SetScheduleChannel", "Sets this channel as the one to do the schedule in.", false)]
        public async Task SetScheduleChannel(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }
            group.ChannelID = ctx.Channel.Id;
            group.MessageID = 0;
            Program.SaveIDs();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Channel {ctx.Channel.Mention} has been selected as the channel to post the schedule in."));
        }

        [SlashCommand("SetAnnouncementChannel", "Sets this channel as the channel used to post announcements about the bot in.", false)]
        public async Task SetAnnouncementChannel(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }
            group.AccouncementID = ctx.Channel.Id;
            Program.SaveIDs();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Channel {ctx.Channel.Mention} has been selected as the channel to post announcements from the bot in."));
        }

        [SlashCommand("ShownStreams", "Sets the amount of streams to be shown from the schedule.", false)]
        public async Task SetNumberOfStreams(InteractionContext ctx, [Option("number","The number of streams to be shown from the schedule. Maximum of 24.")] long number)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }
            if (number < 1)
                group.NumberOfStreams = 1;
            else if (number > 24)
                group.NumberOfStreams = 24;
            else
                group.NumberOfStreams = (int)number;
            Program.SaveIDs();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The number of streams to be shown has been set to {group.NumberOfStreams}"));
        }

        [SlashCommand("SetStreamerTwitch", "Sets which Twitch user to post the schedule from.", false)]
        public async Task SetStreamerTwitch(InteractionContext ctx, [Option("name", "Gets the schedule from the twitch user with this twitch handle.")] string name)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }
            int? id = await TwitchAPI.GetUserID(name.ToLower());
            if (id == null)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Could not find a user with the name {name}"));
            else
            {
                group.BroadcasterID = id.Value;
                RateLimitLease lease = await TwitchAPI.rateLimiter.AcquireAsync(1);
                if (lease.IsAcquired)
                    Program.eventSub.Subscribe.SubscribeToStreamOnline(id.Value.ToString());
                Program.SaveIDs();
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"This server will now display the schedule of the Twitch user {name}"));
            }
        }
    }
}
