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
                        Environment.NewLine +
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

        [Command("pick"), RequireChannel("Lobby"), RequireRole("Registered")]
        public async Task Queue(IUser pickedPlayer)
        {
            //Check if user is the captain of a lobby that's currently picking players.
            var userID = Context.User.Id;
            GameLobby lobby = null;
            foreach (var lob in Session.GameLobbies)
            {
                if (lob.State == GameLobby.LobbyState.PickingPlayers && (lob.Captain1.DiscordID == userID || lob.Captain2.DiscordID == userID))
                    lobby = lob;
            }

            if (lobby != null)
            {
                //Check if it's their turn to pick.
                if ((userID == lobby.Captain1.DiscordID && lobby.PickTurn == 1) || (userID == lobby.Captain2.DiscordID && lobby.PickTurn == 2))
                {
                    //Check if player picked is available.
                    if (lobby.RemainingPlayers.Select(p => p.DiscordID).Contains(pickedPlayer.Id))
                    {
                        //Execute pick.
                        var player = lobby.RemainingPlayers.Where(p => p.DiscordID == pickedPlayer.Id).First();
                        if (userID == lobby.Captain1.DiscordID)
                        {
                            lobby.Team1.Add(player);
                            lobby.RemainingPlayers.Remove(player);
                            lobby.PickTurn = 2;
                        }
                        else
                        {
                            lobby.Team2.Add(player);
                            lobby.RemainingPlayers.Remove(player);
                            lobby.PickTurn = 1;
                        }

                        //Start match once only one player is available.
                        if(lobby.RemainingPlayers.Count == 1)
                        {
                            var lastPlayer = lobby.RemainingPlayers.First();
                            if (lobby.PickTurn == 1)
                                lobby.Team1.Add(lastPlayer);
                            else
                                lobby.Team2.Add(lastPlayer);

                            lobby.RemainingPlayers.Remove(lastPlayer);
                            lobby.State = GameLobby.LobbyState.Playing;

                            var message = MatchStartedMessage(lobby);
                            await SendEmbeddedMessageAsync("Lobby #" + lobby.ID + " - Game Started", message, Colors.Success);
                        }
                        else
                        {
                            //Send Confirmation.
                            var message = PickingPlayersMessage(lobby);
                            await SendEmbeddedMessageAsync("", "Picked <@" + pickedPlayer.Id + ">...", Colors.Success);
                            await SendEmbeddedMessageAsync("Lobby #" + lobby.ID + " - Picking Players", message, Colors.Success);
                        }
                    }
                    else
                        await SendEmbeddedMessageAsync("Command Failed", "Player is not available. Please choose a player from the Remaining Players list.", Colors.Danger);
                }
                else
                    await SendEmbeddedMessageAsync("Command Failed", "It is not your turn to pick.", Colors.Danger);
            }
            else
                await SendEmbeddedMessageAsync("Command Failed", "You are not a captain of an open lobby.", Colors.Danger);
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

        private string PickingPlayersMessage(GameLobby lobby)
        {
            var message = "Team 1" + Environment.NewLine +
                          "Captain: <@" + lobby.Captain1.DiscordID + ">" + Environment.NewLine +
                          "Players: ";

            foreach(var player in lobby.Team1)
            {
                if (player.DiscordID != lobby.Captain1.DiscordID)
                    message += "<@" + player.DiscordID + ">" + Environment.NewLine;
            }

            message += Environment.NewLine + "Team 2" + Environment.NewLine +
                       "Captain: <@" + lobby.Captain2.DiscordID + ">" + Environment.NewLine +
                          "Players: ";

            foreach (var player in lobby.Team2)
            {
                if (player.DiscordID != lobby.Captain2.DiscordID)
                    message += "<@" + player.DiscordID + ">" + Environment.NewLine;
            }

            message += Environment.NewLine + "Remaining Players:" + Environment.NewLine;

            foreach (var player in lobby.RemainingPlayers)
                message += "<@" + player.DiscordID + ">" + Environment.NewLine;
                       
            return message;
        }

        private string MatchStartedMessage(GameLobby lobby)
        {
            var message = "Map: " + lobby.Map.Name + Environment.NewLine +
                          Environment.NewLine +
                          "Team 1" + Environment.NewLine +
                          "Captain: <@" + lobby.Captain1.DiscordID + ">" + Environment.NewLine +
                          "Players: ";

            foreach (var player in lobby.Team1)
            {
                if (player.DiscordID != lobby.Captain1.DiscordID)
                    message += "<@" + player.DiscordID + ">" + Environment.NewLine;
            }

            message += Environment.NewLine + "Team 2" + Environment.NewLine +
                       "Captain: <@" + lobby.Captain2.DiscordID + ">" + Environment.NewLine +
                          "Players: ";

            foreach (var player in lobby.Team2)
            {
                if (player.DiscordID != lobby.Captain2.DiscordID)
                    message += "<@" + player.DiscordID + ">" + Environment.NewLine;
            }

            return message;
        }
    }
}
