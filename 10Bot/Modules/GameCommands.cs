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

            if (userRecord != null)
            {
                await SendEmbeddedMessageAsync("Registration failed.", "You've already registered as a member.", Colors.Danger);
                return;
            }
            else
            {
                var user = Context.User;
                db.Users.Add(new User()
                {
                    DiscordID = user.Id,
                    Username = user.Username,
                    SkillRating = 1500,
                    RatingsDeviation = 350,
                    Volatility = 0.06
                });

                db.SaveChanges();

                var registeredRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Valorant");
                await (Context.User as IGuildUser).AddRoleAsync(registeredRole);
                await SendEmbeddedMessageAsync("Registration Successful!", "All roles have been applied if applicable.", Colors.Success);
            }

        }

        [Command("queue"), RequireChannel("Lobby"), RequireRole("Valorant")]
        [Alias("q")]
        public async Task Queue()
        {
            GameLobby lobby = Session.GetQueuingLobby();
            var discordID = Context.User.Id;

            //Only add user if they aren't already in queue...
            if (Session.IsInActiveLobby(discordID))
            {
                await SendEmbeddedMessageAsync("Command Failed", "You are already in an active lobby or queue.", Colors.Danger);
                return;
            }

            //Make sure user is registered in the database...
            var user = db.Users.Where(u => u.DiscordID == discordID).FirstOrDefault();
            if (user == null)
            {
                await SendEmbeddedMessageAsync("Command Failed", "You're not registered as a user. Please register in the Register channel.", Colors.Info);
                return;
            }

            //Add to queue.
            lobby.AddPlayerToQueue(user);
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

        [Command("force"), RequireChannel("Lobby"), RequireRole("Admin")]
        public async Task Force(IUser forcedPlayer)
        {
            GameLobby lobby = Session.GetQueuingLobby();
            var discordID = forcedPlayer.Id;

            //Only add user if they aren't already in queue...
            if (Session.IsInActiveLobby(discordID))
            {
                await SendEmbeddedMessageAsync("Command Failed", "You are already in an active lobby or queue.", Colors.Danger);
                return;
            }

            //Make sure user is registered in the database...
            var user = db.Users.Where(u => u.DiscordID == discordID).FirstOrDefault();
            if (user == null)
            {
                await SendEmbeddedMessageAsync("Command Failed", "You're not registered as a user. Please register in the Register channel.", Colors.Info);
                return;
            }

            //Add to queue.
            lobby.AddPlayerToQueue(user);
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

        [Command("leave"), RequireChannel("Lobby"), RequireRole("Valorant")]
        [Alias("l")]
        public async Task Leave()
        {
            var discordID = Context.User.Id;
            if (!Session.IsInQueueingLobby(discordID))
            {
                await SendEmbeddedMessageAsync("Command Failed", "You are not currently in a queue.", Colors.Warning);
                return;
            }

            var lobby = Session.GetQueuingLobby();
            lobby.RemovePlayerFromQueue(discordID);

            var playerName = db.Users.Where(u => u.DiscordID == discordID).Select(u => u.Username).First();
            await SendEmbeddedMessageAsync("", playerName + " has left the queue. [" + lobby.Players.Count() + "/10].", Colors.Warning);
        }

        [Command("kick"), RequireChannel("Lobby"), RequireRole("Admin")]
        [Alias("k")]
        public async Task Kick(IUser kickedPlayer)
        {
            var discordID = kickedPlayer.Id;
            if (!Session.IsInQueueingLobby(discordID))
            {
                await SendEmbeddedMessageAsync("Command Failed", "This player is not in a queue.", Colors.Warning);
                return;
            }

            var lobby = Session.GetQueuingLobby();
            lobby.RemovePlayerFromQueue(discordID);

            var playerName = db.Users.Where(u => u.DiscordID == discordID).Select(u => u.Username).First();
            await SendEmbeddedMessageAsync("", playerName + " has been kicked from the queue. [" + lobby.Players.Count() + "/10].", Colors.Warning);
        }

        [Command("fuckofffrank"), RequireChannel("Lobby"), RequireRole("Admin")]
        [Alias("k")]
        public async Task FuckOffFrank()
        {
            var discordID = db.Users.Where(x => x.Username == "getsomeual").Select(x => x.DiscordID).First();
            if (!Session.IsInQueueingLobby(discordID))
            {
                await SendEmbeddedMessageAsync("Command Failed", "Frank is not in a queue.", Colors.Warning);
                return;
            }

            var lobby = Session.GetQueuingLobby();
            lobby.RemovePlayerFromQueue(discordID);

            var playerName = db.Users.Where(u => u.DiscordID == discordID).Select(u => u.Username).First();
            await SendEmbeddedMessageAsync("", playerName + " has been kicked from the queue. [" + lobby.Players.Count() + "/10].", Colors.Warning);
        }

        [Command("clear"), RequireChannel("Lobby"), RequireRole("Admin")]
        public async Task Clear()
        {
            var lobby = Session.GetQueuingLobby();
            lobby.ClearQueue();

            await SendEmbeddedMessageAsync("", "The queue for Lobby #" + lobby.ID + " has been cleared. [" + lobby.Players.Count() + "/10].", Colors.Warning);
        }

        [Command("pick"), RequireChannel("Lobby"), RequireRole("Valorant")]
        [Alias("p")]
        public async Task Pick(IUser pickedPlayer, IUser pickedPlayer2 = null)
        {
            //Check if user is the captain of a lobby that's currently picking players.
            var discordID = Context.User.Id;
            GameLobby lobby = Session.GetLobbyAbleToPickIn(discordID);

            if (lobby == null)
            {
                await SendEmbeddedMessageAsync("Command Failed", "You are not a captain of an open lobby.", Colors.Danger);
                return;
            }

            //Check if it's their turn to pick.
            if (!lobby.IsTurnToPick(discordID))
            {
                await SendEmbeddedMessageAsync("Command Failed", "It is not your turn to pick.", Colors.Danger);
                return;
            }

            //If first pick, must pick one player.
            if (lobby.IsFirstPick() && pickedPlayer2 != null)
            {
                await SendEmbeddedMessageAsync("Command Failed", "First pick must be one player!", Colors.Danger);
                return;
            }

            //If not first pick, must pick two players.
            if(!lobby.IsFirstPick() && pickedPlayer2 == null)
            {
                await SendEmbeddedMessageAsync("Command Failed", "You must pick two players.", Colors.Danger);
                return;
            }

            //Check if players are available.
            if (!lobby.IsAvailablePlayer(pickedPlayer.Id) && (pickedPlayer2 == null ? true : !lobby.IsAvailablePlayer(pickedPlayer2.Id)))
            {
                await SendEmbeddedMessageAsync("Command Failed", "Player is not available. Please choose players from the Remaining Players list.", Colors.Danger);
                return;
            }

            //Execute pick.
            if(pickedPlayer2 == null) //Pick one player.
            {
                lobby.PickPlayer(discordID, pickedPlayer.Id);
                await SendEmbeddedMessageAsync("", "Picked <@" + pickedPlayer.Id + ">...", Colors.Success);
            }
            else //Pick two players.
            {
                lobby.PickPlayer(discordID, pickedPlayer.Id, pickedPlayer2.Id);
                await SendEmbeddedMessageAsync("", "Picked <@" + pickedPlayer.Id + "> and <@" + pickedPlayer2.Id + ">...", Colors.Success);
            }

            //Start match once only one player is available.
            if (lobby.RemainingPlayers.Count == 1)
            {
                lobby.StartMatch();
                await MatchStartEmbeddedMessageAsync(lobby);
            }
            else                
                await PlayerPickedEmbeddedMessageAsync(lobby);
        }

        [Command("report"), RequireChannel("ScoreReport"), RequireRole("Valorant")]
        [Alias("r")]
        public async Task Report(string result)
        {
            result = result.ToLower();

            if (result == "win" || result == "loss")
            {
                var userID = Context.User.Id;
                GameLobby lobby = null;
                foreach (var lob in Session.GameLobbies)
                {
                    if (lob.State == GameLobby.LobbyState.Reporting && (lob.Captain1.DiscordID == userID || lob.Captain2.DiscordID == userID))
                        lobby = lob;
                }

                //Make sure user is captain of a lobby that's currently Playing.
                if (lobby != null)
                {
                    if (lobby.AwaitingConfirmation == false)
                    {
                        if (userID == lobby.Captain1.DiscordID && !lobby.Captain1.HasAlreadyReported)
                        {
                            if (result == "win")
                                lobby.WinningTeam = lobby.Team1;
                            else
                                lobby.WinningTeam = lobby.Team2;

                            lobby.Captain1.HasAlreadyReported = true;
                            lobby.AwaitingConfirmation = true;
                            await SendEmbeddedMessageAsync("", "Awaiting confirmation from remaining Captain...", Colors.Info);
                        }
                        else if (userID == lobby.Captain2.DiscordID && !lobby.Captain2.HasAlreadyReported)
                        {
                            if (result == "win")
                                lobby.WinningTeam = lobby.Team2;
                            else
                                lobby.WinningTeam = lobby.Team1;

                            lobby.Captain2.HasAlreadyReported = true;
                            lobby.AwaitingConfirmation = true;
                            await SendEmbeddedMessageAsync("", "Awaiting confirmation from remaining Captain...", Colors.Info);
                        }
                        else
                            await SendEmbeddedMessageAsync("Command Failed", "You have already reported the result of this match.", Colors.Danger);
                    }
                    else
                    {
                        if (userID == lobby.Captain1.DiscordID && !lobby.Captain1.HasAlreadyReported)
                        {
                            if (result == "win")
                            {
                                if (lobby.WinningTeam == lobby.Team1)
                                {
                                    lobby.ReportResult();
                                    await MatchCompleteEmbeddedMessageAsync(lobby);
                                }
                                else
                                {
                                    lobby.ResetReport();
                                    await SendEmbeddedMessageAsync("Command Failed", "Conflicting results have been reported. Both captains must report the result again, or contact an admin to resolve any conflicts.", Colors.Danger);
                                }
                            }
                            else
                            {
                                if (lobby.WinningTeam == lobby.Team2)
                                {
                                    lobby.ReportResult();
                                    await MatchCompleteEmbeddedMessageAsync(lobby);
                                }
                                else
                                {
                                    lobby.ResetReport();
                                    await SendEmbeddedMessageAsync("Command Failed", "Conflicting results have been reported. Both captains must report the result again, or contact an admin to resolve any conflicts.", Colors.Danger);
                                }
                            }
                        }
                        else if (userID == lobby.Captain2.DiscordID && !lobby.Captain2.HasAlreadyReported)
                        {
                            if (result == "win")
                            {
                                if (lobby.WinningTeam == lobby.Team2)
                                {
                                    lobby.ReportResult();
                                    await MatchCompleteEmbeddedMessageAsync(lobby);
                                }
                                else
                                {
                                    lobby.ResetReport();
                                    await SendEmbeddedMessageAsync("Command Failed", "Conflicting results have been reported. Both captains must report the result again, or contact an admin to resolve any conflicts.", Colors.Danger);
                                }
                            }
                            else
                            {
                                if (lobby.WinningTeam == lobby.Team1)
                                {
                                    lobby.ReportResult();
                                    await MatchCompleteEmbeddedMessageAsync(lobby);
                                }
                                else
                                {
                                    lobby.ResetReport();
                                    await SendEmbeddedMessageAsync("Command Failed", "Conflicting results have been reported. Both captains must report the result again, or contact an admin to resolve any conflicts.", Colors.Danger);
                                }
                            }
                        }
                        else await SendEmbeddedMessageAsync("Command Failed", "You have already reported the result of this match.", Colors.Danger);

                    }
                }
                else
                    await SendEmbeddedMessageAsync("Command Failed", "You must be the captain of a closed lobby to report results.", Colors.Danger);


            }
            else
                await SendEmbeddedMessageAsync("Command Failed", "You must report either a win or a loss. Type !report win or !report loss.", Colors.Danger);
        }

        [Command("result"), RequireChannel("ScoreReport"), RequireRole("Admin")]
        public async Task Result(int lobbyID, int winningTeam)
        {
            //Make sure winningTeam is either 1 or 2.
            if (winningTeam != 1 && winningTeam != 2)
            {
                await SendEmbeddedMessageAsync("Command Failed", "Invaling winning team. Please enter either a 1 or 2.", Colors.Danger);
                return;
            }

            //Find lobby.
            var lobby = Session.GameLobbies.Where(l => l.ID == lobbyID).FirstOrDefault();
            if (lobby == null)
            {
                await SendEmbeddedMessageAsync("Command Failed", "Unable to find lobby.", Colors.Danger);
                return;
            }

            //Make sure lobby is in need of reporting scores.
            if (lobby.State != GameLobby.LobbyState.Reporting)
            {
                await SendEmbeddedMessageAsync("Command Failed", "This lobby is not currently reporting scores.", Colors.Danger);
                return;
            }

            if (winningTeam == 1)
                lobby.WinningTeam = lobby.Team1;
            else
                lobby.WinningTeam = lobby.Team2;

            lobby.ReportResult();
            await MatchCompleteEmbeddedMessageAsync(lobby);
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

        private async Task MatchStartEmbeddedMessageAsync(GameLobby lobby)
        {
            var map = lobby.Map.Name;
            var team1 = "";
            var team2 = "";

            foreach (var player in lobby.Team1)
                team1 += "<@" + player.DiscordID + ">" + Environment.NewLine;
            foreach (var player in lobby.Team2)
                team2 += "<@" + player.DiscordID + ">" + Environment.NewLine;


            var embed = new EmbedBuilder()
                .WithColor(Colors.Info)
                .WithTitle("Lobby #" + lobby.ID + " - Match Started")
                .AddField("Map", map)
                .AddField("Team 1", team1)
                .AddField("Team 2", team2)
                .Build();

            await ReplyAsync("", false, embed);
        }

        private async Task PlayerPickedEmbeddedMessageAsync(GameLobby lobby)
        {
            var team1 = "Captain: <@" + lobby.Captain1.DiscordID + ">" + Environment.NewLine +
                        "Players: ";

            foreach (var player in lobby.Team1)
            {
                if (player.DiscordID != lobby.Captain1.DiscordID)
                    team1 += "<@" + player.DiscordID + ">" + Environment.NewLine;
            }

            var team2 = "Captain: <@" + lobby.Captain2.DiscordID + ">" + Environment.NewLine +
                        "Players: ";

            foreach (var player in lobby.Team2)
            {
                if (player.DiscordID != lobby.Captain2.DiscordID)
                    team2 += "<@" + player.DiscordID + ">" + Environment.NewLine;
            }

            var remainingPlayers = "";
            foreach (var player in lobby.RemainingPlayers)
                remainingPlayers += "<@" + player.DiscordID + ">" + Environment.NewLine;


            var embed = new EmbedBuilder()
                .WithColor(Colors.Info)
                .WithTitle("Lobby #" + lobby.ID + " - Picking Players")
                .AddField("Team 1", team1)
                .AddField("Team 2", team2)
                .AddField("Remaining Players", remainingPlayers)
                .Build();

            await ReplyAsync("", false, embed);
        }

        private async Task MatchCompleteEmbeddedMessageAsync(GameLobby lobby)
        {
            string team1 = "";
            foreach (var player in lobby.Team1)
            {
                var prevRating = Convert.ToInt32(player.PreviousSkillRating);
                var rating = Convert.ToInt32(player.SkillRating);

                team1 += "<@" + player.DiscordID + "> (" + prevRating + " => " + rating + ")" + Environment.NewLine;
            }

            string team2 = "";
            foreach (var player in lobby.Team2)
            {
                var prevRating = Convert.ToInt32(player.PreviousSkillRating);
                var rating = Convert.ToInt32(player.SkillRating);

                team2 += "<@" + player.DiscordID + "> (" + prevRating + " => " + rating + ")" + Environment.NewLine;
            }

            string winningTeam;
            string losingTeam;
            if (lobby.WinningTeam == lobby.Team1)
            {
                winningTeam = team1;
                losingTeam = team2;
            }
            else
            {
                winningTeam = team2;
                losingTeam = team1;
            }

            var embed = new EmbedBuilder()
                .WithColor(Colors.Info)
                .WithTitle("Lobby #" + lobby.ID + " - Match Complete")
                .AddField("Winning Team", winningTeam)
                .AddField("Losing Team", losingTeam)
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}
