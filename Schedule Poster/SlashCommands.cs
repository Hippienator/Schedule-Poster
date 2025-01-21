using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace Schedule_Poster
{
    public class SlashCommands : ApplicationCommandModule
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

        [SlashCommand("UpdateSchedule", "Updates the schedule.", false)]
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
        public async Task SetNumberOfStreams(InteractionContext ctx, [Option("number", "The number of streams to be shown from the schedule. Maximum of 24.")] long number)
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

    [SlashCommandGroup("SetOnlinePing", "These commands are for setting up doing pings when the streamer goes online")]
    public class SetOnlinePing : ApplicationCommandModule
    {

        [SlashCommand("Channel", "Sets the channel that the bot will put a message in when the schedule streamer goes live.", false)]
        public async Task SetOnlinePingChannel(InteractionContext ctx, [Option("channel", "The channel the message goes in. Defaults to the channel it's written in.")] DiscordChannel channel = null)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }

            if (channel == null)
                channel = ctx.Channel;

            DiscordMember botMember = await ctx.Channel.Guild.GetMemberAsync(Program.client.Client.CurrentUser.Id);
            Permissions botPerms = channel.PermissionsFor(botMember);
            if (!botPerms.HasPermission(Permissions.SendMessages))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The bot does not have permissions to send messages in {channel.Mention}."));
                return;
            }
            group.OnlinePingChannelID = channel.Id;
            Program.SaveIDs();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Channel for going online messages set to {channel.Mention}"));
        }

        [SlashCommand("Role", "Sets which role to ping when streamer is going online", false)]
        public async Task SetOnlinePingRole(InteractionContext ctx, [Option("role", "The role that will be pinged.")] DiscordRole role)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }

            group.OnlinePingRoleID = role.Id;
            Program.SaveIDs();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The role that will be pinged is now set to {role.Name}."));
        }

        [SlashCommand("Message", "Sets the message that will be shown when the streamer goes online.", false)]
        public async Task SetOnlinePingMessage(InteractionContext ctx, [Option("message", "The message that will be sent when the streamer goes online.")] string message)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }

            group.OnlinePingMessage = message;
            Program.SaveIDs();

            string reply = "Message text has been updated.";
            if (message.Contains("{ping}") && group.OnlinePingRoleID == null)
                reply += " There is no role selected to ping, but {ping} is in the text. The bot will not send the message until a role is selected with /SetOnlinePing Role .";
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(reply));
        }
        
        [SlashCommand("Enabled", "Sets whether to send a ping when the schedule streamer goes online.", false)]
        public async Task SetOnlinePingEnabled(InteractionContext ctx, [Option("enable", "Set to true to enable the bot to ping when schedule streamer goes online, set to false to disable it")] bool enable)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            IDGroup? group = Program.Groups.Find(x => x.GuildID == ctx.Guild.Id);
            if (group == null)
            {
                group = new IDGroup(ctx.Guild.Id);
                Program.Groups.Add(group);
            }

            if (enable)
            {
                if (group.OnlinePingEnabled)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Pinging when the streamer goes online is already enabled."));
                    return;
                }

                bool failed = false;
                string failMessage = "Please use the command ";
                if (group.OnlinePingMessage == null)
                {
                    failed = true;
                    failMessage += "/SetOnlinePing Message ";
                }
                if (group.OnlinePingChannelID == null)
                {
                    if (failed)
                        failMessage += "and ";
                    failed = true;
                    failMessage += "/SetOnlinePing Channel ";
                }

                if (failed)
                {
                    failMessage += "before enabling this function.";
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(failMessage));
                }
                else
                {
                    group.OnlinePingEnabled = enable;
                    Program.SaveIDs();
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Pinging when the streamer goes online enabled."));
                }

            }
            else
            {
                if (!group.OnlinePingEnabled)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Pinging when the streamer goes online is already disabled."));
                    return;
                }
                group.OnlinePingEnabled = enable;
                Program.SaveIDs();
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Pinging when the streamer goes online disabled."));
            }
        }
    }
}
