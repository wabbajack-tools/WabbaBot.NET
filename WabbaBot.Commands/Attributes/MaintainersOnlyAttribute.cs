using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Core;
using WabbaBot.Core.Exceptions;
using WabbaBot.Models;

namespace WabbaBot.Commands.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class MaintainersOnlyAttribute : SlashCheckBaseAttribute {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ic) {
            // A little backdoor for the bot admins :)
            if (Bot.Settings.Administrators?.Contains(ic.User.Id) ?? false)
                return true;

            var option = ic.Interaction.Data.Options?.FirstOrDefault(option => option.Name == "modlist") ?? default(DiscordInteractionDataOption);
            if (option == default(DiscordInteractionDataOption)) {
                ic.Client.Logger.LogError($"RequireMaintainersOnlyAttribute applied to command {ic.CommandName} but no modlist option found! Failed to execute command.");
            }
            using (var dbContext = new BotDbContext()) {
                var maintainer = dbContext.Maintainers.FirstOrDefault(maintainer => maintainer.DiscordUserId == ic.User.Id);
                if (maintainer != default(Maintainer)) {
                    dbContext.Entry(maintainer).Collection(m => m.ManagedModlists).Load();
                    return maintainer.ManagedModlists.Exists(mm => mm.MachineURL == (string)option.Value);
                }
                else {
                    throw new MaintainersOnlyCommandException(ic);
                }
            }
        }
    }
}
