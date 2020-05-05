using _10Bot.Classes;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace _10Bot.Services
{
    public class QueueService
    {
        private readonly DiscordSocketClient client;
        private readonly Timer timer;
        private readonly TimeSpan queueTimeout;
        private readonly AppConfig appConfig;

        public QueueService(DiscordSocketClient client, IOptions<AppConfig> appConfig)
        {
            this.client = client;
            this.appConfig = appConfig.Value;
            timer = new Timer(RunQueueCheck, null, 5000, 1000 * 60 * 15); //RunQueueCheck fires every 15 minutes.
            queueTimeout = TimeSpan.FromHours(1); //Queue timeout set for one hour.
        }

        public void RunQueueCheck(object stateInfo = null)
        {
            var currentTime = DateTime.Now;
            var queuedPlayers = Session.GetQueuedPlayers();

            foreach (var player in queuedPlayers)
            {
                if (player.QueuedAt + queueTimeout < currentTime)
                {
                    var lobby = Session.GetQueuingLobby();
                    lobby.RemovePlayerFromQueue(player.DiscordID);

                    var channel = client.GetChannel(appConfig.LobbyChannel) as SocketTextChannel;
                    if (channel != null)
                    {
                        var embed = new EmbedBuilder()
                                        .WithColor(Colors.Warning)
                                        .WithDescription(player.Username + " has been timed out from the queue. [" + lobby.Players.Count + "/10]")
                                        .Build();

                        channel.SendMessageAsync(null, false, embed, null);
                    }
                }
            }
        }
    }
}
