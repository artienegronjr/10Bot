using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace _10Bot
{
    public class RequireChannelAttribute : PreconditionAttribute
    {
        private readonly string _channelName;

        public RequireChannelAttribute(string channelName)
        {
            _channelName = channelName;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            AppConfig appConfig = services.GetService<IOptions<AppConfig>>().Value;
            var channelID = appConfig.GetType().GetProperty(_channelName + "Channel").GetValue(appConfig) as ulong?;

            typeof(AppConfig).GetProperties();
            
            if (context.Channel.Id == channelID)
                return await Task.FromResult(PreconditionResult.FromSuccess());
            else
                return await Task.FromResult(PreconditionResult.FromError("This command must be executed in the " + _channelName + " channel."));
        }
    }
}