using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Core;
using WabbaBot.Models;

namespace WabbaBot.Commands.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class RequireMaintainersOnlyAttribute : SlashCheckBaseAttribute {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
            // A little backdoor for the bot admins :)
            if (Bot.Settings.Administrators?.Contains(ctx.User.Id) ?? false)
                return Task.FromResult(true);

            var option = ctx.Interaction.Data.Options?.FirstOrDefault(option => option.Name == "modlist") ?? default(DiscordInteractionDataOption);
            if (option == default(DiscordInteractionDataOption)) {
                ctx.Client.Logger.LogError($"RequireMaintainersOnlyAttribute applied to command {ctx.CommandName} but no modlist option found! Failed to execute command.");
            }
            using (var dbContext = new BotDbContext()) {
                var maintainer = dbContext.Maintainers.FirstOrDefault(maintainer => maintainer.DiscordUserId == ctx.User.Id);
                if (maintainer != default(Maintainer)) {
                    dbContext.Entry(maintainer).Collection(m => m.ManagedModlists).Load();
                    return Task.FromResult(maintainer.ManagedModlists.Exists(mm => mm.MachineURL == (string)option.Value));
                }
            }
            return Task.FromResult(false);
        }
    }
}
