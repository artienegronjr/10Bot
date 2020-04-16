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
            _config = BuildConfig();
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = ConfigureServices();

            _client.Log += _client_Log;

            var appConfig = _services.GetService<IOptions<AppConfig>>().Value;

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

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            //Ignore messages from other Discord bots.
            if (message.Author.IsBot) return;

            int argPos = 0;
            //Set command prefix.
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
