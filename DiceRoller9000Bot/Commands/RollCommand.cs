using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;

namespace DiceRoller9000Bot.Commands
{
    public class RollCommand:BaseCommandModule
    {
        private readonly ILogger<RollCommand> _logger;

        public RollCommand(ILogger<RollCommand> logger)
        {
            _logger = logger;
        }

        [Command]
        [Description("Rolls the dice.")]
        [Aliases("r")]
        public async Task Roll(CommandContext context, [Description("The roll request, given in <number of dice>d<die size> for example 1d10 to roll a single 10 sided die.")]
            string rollRequest, [Description("Should another die be added to the pool if the max value is rolled? true/false Default: true")]
            bool explode = true, [Description("Should the sum of the dice be shown? true/false")]
            bool sum = false)
        {
            string whoRolled = string.Empty;

            if (context.Member != null)
            {
                whoRolled = string.IsNullOrWhiteSpace(context.Member.Nickname) ? context.Member.DisplayName : context.Member.Nickname;
            }

            _logger.LogInformation("Received roll command from {@whoRolled}, request: {@rollRequest} explode:{@explode} sum:{@sum}", whoRolled, rollRequest, explode, sum);

            await context.TriggerTypingAsync();

            (int count, int size) = ParseRollRequest(rollRequest);

            if (count <= 0 || size <= 0)
            {
                _logger.LogWarning("{whoRolled} sent {rollRequest}, it was parsed as {count}d{size}", whoRolled, rollRequest, count, size);
                await context.RespondAsync("Hey what gives! Try again, format is #d#, like 10d6");
            }
            else
            {

                (List<int> rolls, int explosionCount) = RollDice(count, size, explode);
                var responseBuilder = new StringBuilder();

                responseBuilder.AppendLine($"{whoRolled} rolled {count}d{size}!");
                if (sum)
                {
                    responseBuilder.AppendLine($"Total: {rolls.Sum()}");
                }

                if (explode && explosionCount > 0)
                {
                    responseBuilder.AppendLine($"{explosionCount} explosions!");
                }

                responseBuilder.AppendLine($"({string.Join(", ", rolls.OrderByDescending(i => i))})");
                _logger.LogInformation("Rolls complete, responding with {response}",responseBuilder.ToString());
                await context.RespondAsync(responseBuilder.ToString());
            }
        }

        private (List<int> Rolls, int ExplosionCount) RollDice(int rollRequestCount, int rollRequestSize, bool explode)
        {
            _logger.LogTrace("Rolling {rollRequestCount}d{rollRequestSize}, explode:{explode}", rollRequestCount, rollRequestSize, explode);
            if (rollRequestSize < 2)
            {
                explode = false;
            }

            var rolls = new List<int>(rollRequestCount);
            var explosionCount = 0;
            for (var i = 0; i < rollRequestCount; i++)
            {
                int roll = RandomNumberGenerator.GetInt32(1, rollRequestSize + 1);
                rolls.Add(roll);

                while (roll == rollRequestSize && explode)
                {
                    _logger.LogInformation("Rolled a {roll} on a {rollRequestSize} resulting in an explosion", roll, rollRequestSize);
                    explosionCount++;
                    roll = RandomNumberGenerator.GetInt32(1, rollRequestSize + 1);
                    rolls.Add(roll);
                }
            }

            return (rolls, explosionCount);
        }

        private static (int count, int size) ParseRollRequest(string request)
        {
            (int, int) badResult = (-1, -1);
            
            if (!request.ToLowerInvariant().Contains("d"))
            {
                return badResult;
            }

            string[] split = request.ToLowerInvariant().Split('d');

            if (split.Length != 2)
            {
                return badResult;
            }

            if (!int.TryParse(split[0], out int count) || !int.TryParse(split[1], out int size))
            {
                return badResult;
            }

            return (count, size);
        }
    }
}