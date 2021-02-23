using System;
using System.Threading.Tasks;
using DiceRoller9000Bot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiceRoller9000Bot
{
public class DiceRoller9000
    {
        private readonly IServiceProvider _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public DiceRoller9000(IServiceProvider services,ILoggerFactory loggerFactory,IConfiguration configuration, ILogger<DiceRoller9000> logger)
        {
            _services = services;
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _logger = logger;
            
        }

        public async Task Run()
        {
            _logger.LogInformation("Dice roller started.");
            DiscordConfiguration discordConfiguration = LoadDiscordConfiguration();
            CommandsNextConfiguration commandsConfiguration =  LoadCommandsNextConfiguration();
            
            var discord = new DiscordClient(discordConfiguration);


            CommandsNextExtension commands = discord.UseCommandsNext(commandsConfiguration);

            commands.RegisterCommands<RollCommand>();
            commands.RegisterCommands<RuleCommand>();

            _logger.LogInformation("Connecting to Discord");

            await discord.ConnectAsync();

            _logger.LogInformation("Connected to Discord");

            await Task.Delay(-1);
        }


        private CommandsNextConfiguration LoadCommandsNextConfiguration()
        {
            _logger.LogInformation("Loading Command Configuration");

            IConfigurationSection section = _configuration.GetSection("CommandsNextConfiguration");
            string[] stringPrefixes = section.GetSection("StringPrefixes").Get<string[]>();
            if (stringPrefixes.Length == 0)
            {
                stringPrefixes = new[] {";;"};
            }

            var config = new CommandsNextConfiguration
            {
                Services = _services,
                CaseSensitive = section.GetValue("CaseSensitive", false),
                EnableDefaultHelp = section.GetValue("EnableDefaultHelp", true),
                EnableDms = section.GetValue("EnableDms", true),
                EnableMentionPrefix = section.GetValue("EnableMentionPrefix", true),
                IgnoreExtraArguments = section.GetValue("IgnoreExtraArguments", true),
                DmHelp = section.GetValue("DmHelp ", true),
                UseDefaultCommandHandler = section.GetValue("IgnoreExtraArguments", true),
                StringPrefixes = stringPrefixes
            };
            _logger.LogInformation("Command Configuration loaded {@config}", config);
            return config;
        }

        private DiscordConfiguration LoadDiscordConfiguration()
        {
            _logger.LogInformation("Loading Discord Configuration");

            IConfigurationSection section = _configuration.GetSection("DiscordConfiguration");
            var config = new DiscordConfiguration
            {
                LoggerFactory = _loggerFactory,
                Token = section.GetValue<string>("Token"),
                TokenType = section.GetValue("TokenType", TokenType.Bot),
                AutoReconnect = section.GetValue("AutoReconnect", true),
                LargeThreshold = section.GetValue("LargeThreshold", 250),
                MessageCacheSize = section.GetValue("MessageCacheSize", 1024),
                ShardCount = section.GetValue("ShardCount", 1),
                ShardId = section.GetValue("ShardId", 0)
            };

            _logger.LogInformation("Discord Configuration loaded {@config}", config);
            
            return config;
        }

    }
}
