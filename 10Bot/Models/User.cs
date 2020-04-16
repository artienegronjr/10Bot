using System;
using System.Collections.Generic;
using System.Text;

namespace _10Bot.Models
{
    public class User
    {
        public int ID { get; set; }
        public long? DiscordID { get; set; }
        public string Username { get; set; }
        public double SkillRating { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}
