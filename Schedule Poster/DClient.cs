using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Schedule_Poster.Logging;

namespace Schedule_Poster
{
    internal class DClient
    {
        public DiscordClient Client;
        public bool zombied = false;

        public DClient()
        {
            DiscordConfiguration config = new DiscordConfiguration() { Token = Program.Token, Intents = DiscordIntents.AllUnprivileged};
            Client = new DiscordClient(config);
            SlashCommandsExtension slash = Client.UseSlashCommands();
            slash.RegisterCommands<SlashCommands>();
            slash.RegisterCommands<AdminCommands>(Program.mainID.GuildID);
            Client.SocketClosed += Client_SocketClosed;
            Client.SocketErrored += Client_SocketErrored;
            Client.Ready += Client_Ready;
            Client.SocketOpened += Client_SocketOpened;
            Client.Zombied += Client_Zombied;
        }

        private Task Client_Zombied(DiscordClient sender, DSharpPlus.EventArgs.ZombiedEventArgs args)
        {
            if (zombied)
            {
                Logger.Log("[Critical]DSharp client zombiefied 5 times, starting reconnection timer.");
                Program.reconnectionTimer.Start();
            }
            return Task.CompletedTask;
        }

        private Task Client_SocketOpened(DiscordClient sender, DSharpPlus.EventArgs.SocketEventArgs args)
        {
            Logger.Log($"[Info]DSharp socket opened.");
            return Task.CompletedTask;
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            Logger.Log($"[Starting]DSharp socket connected and ready.");
            return Task.CompletedTask;
        }

        private Task Client_SocketErrored(DiscordClient sender, DSharpPlus.EventArgs.SocketErrorEventArgs args)
        {
            Logger.Log($"[Critical]DSharp socket Errored. Reason: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task Client_SocketClosed(DiscordClient sender, DSharpPlus.EventArgs.SocketCloseEventArgs args)
        {
            Logger.Log($"[Critical]DSharp socket closed. Reason: {args.CloseMessage}");
            return Task.CompletedTask;
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
