using System;
using System.Collections.Generic;
using System.Text;
using _10Bot.Models;
using Discord;
using Discord.WebSocket;
using System.Linq;

namespace _10Bot.Classes
{
    public class GameLobby
    {
        public enum LobbyState
        {
            Queuing,
            PickingPlayers,
            Playing,
            Reporting,
            Complete
        }
        public int ID { get; set; }
        public List<User> Players { get; set; }
        public List<User> Team1 { get; set; }
        public List<User> Team2 { get; set; }
        public User Captain1 { get; set; }
        public User Captain2 { get; set; }
        public Map Map { get; set; }
        public LobbyState State { get; set; }

        private readonly EFContext db;
        private readonly Random random;

        public GameLobby()
        {
            db = new EFContext();
            this.random = new Random();

            Players = new List<User>();
            Team1 = new List<User>();
            Team2 = new List<User>();
        }

        public void PopQueue()
        {
            ChooseCaptains();
            ChooseMap();

            State = LobbyState.PickingPlayers;
        }

        public void ChooseCaptains()
        {

        }

        private void ChooseMap()
        {
            var maps = db.Maps.ToList();
            var index = random.Next(maps.Count);

            Map = maps[index];
        }
    }
}
