using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace _10Bot.Modules
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        [Command("info"), AllowedChannelsService]
        public async Task Info()
        {
            await ReplyAsync("Info...");
        }
    }
}