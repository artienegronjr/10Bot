using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace _10Bot.Modules
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        private readonly Color EMBED_MESSAGE_COLOR = new Color(120, 40, 40);

        [Command("info"), AllowedChannelsService]
        public async Task Info()
        {
            await SendEmbeddedMessageAsync("Registration Successful!", "All roles have been applied if applicable.");
        }

        async Task SendEmbeddedMessageAsync(string title, string message)
        {
            var embed = new EmbedBuilder()
                .WithColor(EMBED_MESSAGE_COLOR)
                .WithTitle(title)
                .WithDescription(message)
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}