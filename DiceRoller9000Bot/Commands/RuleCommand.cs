using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiceRoller9000Bot.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using RulesEngine;
using RulesEngine.Models;

namespace DiceRoller9000Bot.Commands
{
    public class RuleCommand:BaseCommandModule
    {
        private readonly IRuleCollection _rules;
        private readonly ILogger<RuleCommand> _logger;

        public RuleCommand(IRuleCollection rules,ILogger<RuleCommand> logger)
        {
            _rules = rules;
            _logger = logger;
        }

        [Command]
        [Description("Looks up a rule.")]
        public async Task Rule(CommandContext context, [RemainingText] [Description("The string to search for.")]string query)
        {
            if (query.Length < 3)
            {
                _logger.LogInformation("Short query received. '{query}'", query);
                await context.RespondAsync($"Minimum query length is 3 characters.");
                return;
            }
            try
            {
                IEnumerable<SearchResult> results = _rules.Lookup(query);
                SearchResult[] searchResults = results as SearchResult[] ?? results.ToArray();
                SearchResult exactMatch = searchResults.FirstOrDefault(r => r.ExactMatch);
                if (exactMatch != null || searchResults.Length == 1)
                {

                    SearchResult result = exactMatch ?? searchResults[0];
                    await RespondWithRule(context, result.Entry);
                }
                else if(searchResults.Length>0)
                {
                    await RespondWithPartialResults(context, searchResults);
                }
                else
                {
                    await context.RespondAsync($"No results for '{query}'");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while generating the response, {@ruleName}", query);
                throw;
            }
        }

        private async Task RespondWithPartialResults(CommandContext context, SearchResult[] results)
        {
            _logger.LogInformation("Responding to rule request with partial matches {@results}", results);
            var builder = new DiscordEmbedBuilder();

            builder.WithColor(DiscordColor.NotQuiteBlack);
            builder.WithDescription("Multiple search results found. For a detailed explanation use the command again with the full name of the rule. Search Results are limited to the first 25 results.");
            builder.WithTitle("Rule Search Results");
            foreach (SearchResult result in results.Take(25))
            {
                builder.AddField(result.Entry.Name, result.Entry.Description.TruncateForDisplay(1024, "(more)"));
            }

            await context.RespondAsync(embed: builder.Build());
        }

        private async Task RespondWithRule(CommandContext context, Entry resultEntry)
        {
            _logger.LogInformation("Responding to the rule request, {@resultEntry}", resultEntry);

            var builder = new DiscordEmbedBuilder();

            builder.WithColor(GetColor(resultEntry.Type));
            builder.WithTitle(resultEntry.Name);
            builder.WithFooter($"{resultEntry.Source} pg.{resultEntry.Page}");

            builder.WithDescription(resultEntry.Description.TruncateForDisplay(2048, "(more)"));

            if (resultEntry.Examples != null && resultEntry.Examples.Length > 0)
            {
                var sb = new StringBuilder();
                foreach (string example in resultEntry.Examples)
                {
                    sb.AppendLine($"> {example}");
                }

                builder.AddField("Examples", sb.ToString());
            }

            if (!string.IsNullOrWhiteSpace(resultEntry.Note))
            {
                builder.AddField("Note", resultEntry.Note);
            }
            if (!string.IsNullOrWhiteSpace(resultEntry.Calling))
            {
                builder.AddField("Calling", resultEntry.Calling);
            }
            if (!string.IsNullOrWhiteSpace(resultEntry.InnatePower))
            {
                builder.AddField("Innate Power", resultEntry.InnatePower);
            }

            DiscordEmbed response = builder.Build();

            await context.RespondAsync(embed: response);

            _logger.LogInformation("Building tables");

            foreach (Table table in resultEntry.Tables)
            {
                var tableBuilder = new StringBuilder();
                var renderer = new TableRenderer();

                if (table.Headers != null)
                {
                    renderer.SetHeader(table.Headers);
                }

                foreach (Row row in table.Rows)
                {
                    renderer.AddRow(row.Columns);
                }

                tableBuilder.AppendLine();
                tableBuilder.AppendLine(Formatter.BlockCode(renderer.Build()));
                await context.RespondAsync(tableBuilder.ToString());
            }
        }

        private static DiscordColor GetColor(EntryType entryType)
        {
            switch (entryType)
            {
                case EntryType.Rule:
                    return DiscordColor.Red;
                case EntryType.Skill:
                    return DiscordColor.Blurple;
                case EntryType.Knack:
                    return DiscordColor.DarkGreen;
                case EntryType.Purview:
                    return DiscordColor.Azure;
            }

            return DiscordColor.NotQuiteBlack;
        }
    }
}
