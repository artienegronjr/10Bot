using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace _10Bot
{
    public class AllowedChannelsService : PreconditionAttribute
    {
        private readonly RequireContextAttribute _contextType = new RequireContextAttribute(ContextType.DM);

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            AppConfig appConfig = services.GetService<IOptions<AppConfig>>().Value;

            var isDM = await _contextType.CheckPermissionsAsync(context, command, services);

            //Only allow commands to execute if typed in the proper channel, or if it's a direct DM.
            if (context.Channel.Id == appConfig.AllowedChannel || isDM.IsSuccess)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("");
        }
    }
}
