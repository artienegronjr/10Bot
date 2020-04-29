using System;
using System.Collections.Generic;
using System.Text;
using _10Bot.Models;
using _10Bot.Classes;
using Discord;
using Discord.WebSocket;
using System.Linq;
using _10Bot.Services;

namespace _10Bot
{
    public static class Session
    {
        public static List<GameLobby> GameLobbies { get; set; }
        public static int LobbiesCreated { get; set; }

        static Session()
        {
            GameLobbies = new List<GameLobby>();
            LobbiesCreated = 0;
        }

        public static GameLobby CreateNewLobby()
        {
            LobbiesCreated += 1;

            var lobby = new GameLobby()
            {
                ID = LobbiesCreated,
                State = GameLobby.LobbyState.Queuing
            };

            GameLobbies.Add(lobby);

            return lobby;
        }
        public static GameLobby GetQueuingLobby()
        {
            //Find a lobby that's currently queuing. If unable to, create one.
            foreach (var lobby in GameLobbies)
            {
                if (lobby.State == GameLobby.LobbyState.Queuing)
                    return lobby;
            }

            return CreateNewLobby();
        }
        public static bool IsInActiveLobby(ulong discordID)
        {
            foreach (var lobby in GameLobbies)
            {
                if (lobby.State != GameLobby.LobbyState.Complete && lobby.Players.Select(p => p.DiscordID).Contains(discordID))
                    return true;
            }

            return false;
        }
        public static bool IsInQueueingLobby(ulong discordID)
        {
            foreach (var lobby in GameLobbies)
            {
                if (lobby.State == GameLobby.LobbyState.Queuing && lobby.Players.Select(p => p.DiscordID).Contains(discordID))
                    return true;
            }

            return false;
        }
        public static GameLobby GetLobbyAbleToPickIn(ulong discordID)
        {
            foreach (var lobby in GameLobbies)
            {
                if (lobby.State == GameLobby.LobbyState.PickingPlayers && lobby.IsACaptain(discordID))
                    return lobby;
            }

            return null;
        }
        public static List<User> GetQueuedPlayers()
        {
            var queuedPlayers = new List<User>();
            var lobby = GetQueuingLobby();

            foreach (var player in lobby.Players)
            {
                queuedPlayers.Add(player);
            }

            return queuedPlayers;
        }
    }
}
