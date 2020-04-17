using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace _10Bot
{
    public class RequireRegisterChannel : PreconditionAttribute
    {
        private readonly RequireContextAttribute _contextType = new RequireContextAttribute(ContextType.DM);

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            AppConfig appConfig = services.GetService<IOptions<AppConfig>>().Value;

            if (context.Channel.Id == appConfig.RegisterChannel)
                return await Task.FromResult(PreconditionResult.FromSuccess());
            else
                return await Task.FromResult(PreconditionResult.FromError(""));
        }
    }
}