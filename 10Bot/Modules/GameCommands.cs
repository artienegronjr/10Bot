using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using _10Bot.Models;
using _10Bot.Classes;

namespace _10Bot.Modules
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        private readonly EFContext db;

        public GameCommands()
        {
            db = new EFContext();
        }

        [Command("register"), RequireRegisterChannel]
        public async Task Register()
        {
            var userID = Context.User.Id;
            var userRecord = db.Users.Where(u => u.DiscordID == userID).FirstOrDefault();

            if(userRecord == null)
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

        async Task SendEmbeddedMessageAsync(string title, string message, Color color)
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
