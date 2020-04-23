using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10Bot.Models
{
    public class User
    {
        public int ID { get; set; }
        public ulong DiscordID { get; set; }
        public string Username { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double SkillRating { get; set; }
        public double PreviousSkillRating { get; set; }
        public double RatingsDeviation { get; set; }
        public double Volatility { get; set; }


        [NotMapped]
        public bool HasAlreadyReported { get; set; }
    }
}
