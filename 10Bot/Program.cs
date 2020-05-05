using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using _10Bot.Services;
using _10Bot.Classes;

namespace _10Bot
{
    class Program
    {
        private IConfigurationRoot _config;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {          
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = ConfigureServices();
            _config = BuildConfig();

            _client.Log += _client_Log;

            var appConfig = _services.GetService<IOptions<AppConfig>>().Value;
            var queueService = _services.GetService<QueueService>();

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, appConfig.DiscordBotToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<QueueService>()
                .Configure<AppConfig>(options => _config.GetSection("AppConfig").Bind(options))
                .BuildServiceProvider();
        }

        private IConfigurationRoot BuildConfig()
        {
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"Loading config.json from: {folder}");
            string path = Path.Combine(folder, "config.json");
            if (!File.Exists(path))
            {
                Console.WriteLine("Can't find config.json at path.");
            }

            return new ConfigurationBuilder()
                .SetBasePath(folder)
                .AddJsonFile("config.json")
                .Build();
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            //Ignore messages from other Discord bots.
            if (message.Author.IsBot) return;

            //Set command prefix.
            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We have access to the information of the command executed,
            // the context of the command, and the result returned from the
            // execution in this event.

            // We can tell the user what went wrong
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            // ...or even log the result (the method used should fit into
            // your existing log handler)
            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            await _client_Log(new LogMessage(LogSeverity.Info,
                "CommandExecution",
                $"{commandName} was executed at {DateTime.UtcNow}."));
        }
    }
}
