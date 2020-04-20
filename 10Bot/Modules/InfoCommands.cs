using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using _10Bot.Classes;

namespace _10Bot.Modules
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {

        [Command("info")]
        public async Task Info()
        {
            var message = "Captains have been picked." +
                Environment.NewLine +
                "First Captain: Test" +
                Environment.NewLine +
                "Second Captain: Test";

            await SendEmbeddedMessageAsync("", message, Colors.Info);
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
    }
}