using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using _10Bot.Classes;
using System.Linq;
using _10Bot.Models;
using System.IO;

namespace _10Bot.Modules
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        private readonly EFContext db;

        public InfoCommands()
        {
            db = new EFContext();
        }

        [Command("profile"), RequireContext(ContextType.DM)]
        public async Task Profile()
        {
            var userID = Context.User.Id;
            var userRecord = db.Users.Where(u => u.DiscordID == userID).FirstOrDefault();

            if (userRecord == null)
            {
                await SendEmbeddedMessageAsync("Command Failed.", "You are not registered as a user.", Colors.Danger);
                return;
            }
            else
            {
                await ProfileEmbeddedMessageAsync(userRecord);
            }
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

        private async Task ProfileEmbeddedMessageAsync(User user)
        {
            var record = "Total Games: " + (user.Wins + user.Losses) + Environment.NewLine +
                         "Wins: " + user.Wins + Environment.NewLine +
                         "Losses: " + user.Losses + Environment.NewLine;

            var rank = "Skill Rating: " + Convert.ToInt32(user.SkillRating);

            var embed = new EmbedBuilder()
                .WithColor(Colors.Info)
                .WithTitle("Player Profile")
                .AddField("Record", record)
                .AddField("Rank", rank)
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}