using System;
using System.Collections.Generic;
using System.Text;
using _10Bot.Models;
using _10Bot.Classes;
using Discord;
using Discord.WebSocket;
using System.Linq;

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
        public static bool AllLobbiesFull()
        {
            for(var i = 0; i<GameLobbies.Count; i++)
            {
                if (GameLobbies[i].State == GameLobby.LobbyState.Queuing)
                    return false;
            }

            return true;
        }
        public static GameLobby GetCurrentlyQueuingLobby()
        {
            foreach(var lobby in GameLobbies)
            {
                if (lobby.State == GameLobby.LobbyState.Queuing)
                    return lobby;
            }

            return null;
        }

        public static bool IsInOpenLobby(ulong discordID)
        {
            foreach(var lobby in GameLobbies)
            {
                if (lobby.Players.Select(p => p.DiscordID).Contains(discordID) && lobby.State != GameLobby.LobbyState.Complete)
                    return true;
            }

            return false;
        }
    }
}
