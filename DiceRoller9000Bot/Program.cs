using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine;
using Serilog;

namespace DiceRoller9000Bot
{
    internal static class Program
    {
        private static async Task Main()
        {
            ServiceProvider services = RegisterServices();
            
            var roller = ActivatorUtilities.CreateInstance<DiceRoller9000>(services);

            await roller.Run();

            await Task.Delay(-1);

        }

        private static ServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
            services.AddSingleton<IConfiguration>(provider => config);
            services.AddSingleton<IRuleCollection>(provider =>
            {
                var collection = new RuleCollection();
                collection.LoadEntries(config.GetValue("RuleFile", "rules.json"));
                return collection;
            });
            services.AddLogging(builder => builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger()));
            return services.BuildServiceProvider();
        }

    }
}
