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
            //Try to find captains with at least 15 games played...
            var captainPool = Players.Where(p => (p.Wins + p.Losses) >= 15);
            if (captainPool.Count() < 2)
                captainPool = Players;

            //Find at most five of the highest rated players in the Captain pool.
            int num = Math.Min(captainPool.Count(), 5);
            var orderList = captainPool.OrderByDescending(c => c.SkillRating).Take(num);

            //First captain is randomly chosen.
            int capt1Index = random.Next(orderList.Count());
            Captain1 = orderList.ToList()[capt1Index];

            //Second captain is the player closest in rating to the first captain.
            int indexAbove = capt1Index - 1;
            int indexBelow = capt1Index + 1;

            double skillDiffAbove = double.MaxValue;
            double skillDiffBelow = double.MaxValue;

            var playerAbove = orderList.ElementAtOrDefault(indexAbove);
            var playerBelow = orderList.ElementAtOrDefault(indexBelow);

            if (playerAbove != null)
                skillDiffAbove = Math.Abs(Captain1.SkillRating - playerAbove.SkillRating);
            if (playerBelow != null)
                skillDiffBelow = Math.Abs(Captain1.SkillRating - playerBelow.SkillRating);

            if (skillDiffAbove < skillDiffBelow)
                Captain2 = playerAbove;
            else
                Captain2 = playerBelow;

            //Make sure Captain with lower rating has first pick.
            if(Captain1.SkillRating > Captain2.SkillRating)
            {
                var temp = Captain1;
                Captain1 = Captain2;
                Captain2 = temp;
            }
        }

        private void ChooseMap()
        {
            var maps = db.Maps.ToList();
            var index = random.Next(maps.Count);

            Map = maps[index];
        }
    }
}
