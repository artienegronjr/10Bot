using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using _10Bot.Models;
using _10Bot.Classes;
using Microsoft.Extensions.Options;
using _10Bot.Preconditions;

namespace _10Bot.Modules
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        private readonly EFContext db;
        private readonly AppConfig appConfig;

        public GameCommands(IOptions<AppConfig> appConfig)
        {
            db = new EFContext();
            this.appConfig = appConfig.Value;
        }

        [Command("register"), RequireChannel("Register")]
        public async Task Register()
        {
            var userID = Context.User.Id;
            var userRecord = db.Users.Where(u => u.DiscordID == userID).FirstOrDefault();

            if (userRecord == null)
            {
                var user = Context.User;

                db.Users.Add(new User()
                {
                    DiscordID = user.Id,
                    Username = user.Username
                });

                db.SaveChanges();

                var registeredRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Registered");
                await (Context.User as IGuildUser).AddRoleAsync(registeredRole);


                await SendEmbeddedMessageAsync("Registration Successful!", "All roles have been applied if applicable.", Colors.Success);
            }
            else
                await SendEmbeddedMessageAsync("Registration failed.", "You've already registered as a member.", Colors.Danger);
        }

        [Command("queue"), RequireChannel("Lobby"), RequireRole("Registered")]
        public async Task Queue()
        {
            //Find a lobby that's currently queuing, or create one if unable to find one.
            GameLobby lobby;
            if (Session.GameLobbies.Count == 0 || Session.AllLobbiesFull())
                lobby = Session.CreateNewLobby();
            else
                lobby = Session.GetCurrentlyQueuingLobby();

            var discordID = Context.User.Id;

            //Only add user if they aren't already in queue.
            if (!lobby.Players.Select(p => p.DiscordID).Contains(discordID) && !Session.IsInLobby(discordID))
            {
                var user = db.Users.Where(u => u.DiscordID == discordID).FirstOrDefault();
                lobby.Players.Add(user);
                await SendEmbeddedMessageAsync("", user.Username + " has joined the queue for Lobby #" + lobby.ID + ". [" + lobby.Players.Count + "/10]", Colors.Success);

                //Pop queue if queue reaches maximum size.
                if (lobby.Players.Count == appConfig.PlayersPerTeam * 2)
                {
                    await SendEmbeddedMessageAsync("", "Queue is full. Picking teams...", Colors.Info);
                    lobby.PopQueue();

                    var message = "Captains have been picked for Lobby #" + lobby.ID + "." + Environment.NewLine +
                        "Team #1 Captain: <@" + lobby.Captain1.DiscordID + ">" + Environment.NewLine +
                        "Team #2 Captain: <@" + lobby.Captain2.DiscordID + ">" + Environment.NewLine +
                        Environment.NewLine +
                        "Remaining Players:" + Environment.NewLine +
                        Environment.NewLine;

                    foreach (var player in lobby.Players)
                    {
                        if (player.DiscordID != lobby.Captain1.DiscordID && player.DiscordID != lobby.Captain2.DiscordID)
                            message += "<@" + player.DiscordID + ">" + Environment.NewLine;
                    }

                    await SendEmbeddedMessageAsync("Lobby #" + lobby.ID + " - Picking Teams", message, Colors.Info);
                    await SendEmbeddedMessageAsync("", "First pick goes to <@" + lobby.Captain1.DiscordID + ">. Use the !pick command to select a player.", Colors.Info);
                }
            }
            else
                await SendEmbeddedMessageAsync("Command Failed", "You are already in an active lobby or queue.", Colors.Danger);
        }

        private async Task SendEmbeddedMessageAsync(string title, string message, Color color)
        {
            var embed = new EmbedBuilder()
                .WithColor(color)
                .WithTitle(title)
                .WithDescription(message)
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}
