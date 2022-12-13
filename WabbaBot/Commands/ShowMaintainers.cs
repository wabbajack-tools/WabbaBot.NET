using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(ShowMaintainers), "Show everyone maintaining a specific modlist")]
        public async Task ShowMaintainers(InteractionContext ic, [Option("Modlist", "The modlist you want to show the maintainers for", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.Include(m => m.Maintainers).FirstOrDefault(m => m.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist with machineURL **{machineURL}** not found.");
                    return;
                }
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"Modlist **{machineURL}** is being maintained by {managedModlist.Maintainers.Count} maintainer(s): ");
                Parallel.ForEach(managedModlist.Maintainers, maintainer => {
                    var discordUser = ic.Client.GetUserAsync(maintainer.DiscordUserId).Result;
                    if (discordUser != null)
                        messageBuilder.AppendLine($"**{discordUser.Username}#{discordUser.Discriminator}** (`{discordUser.Id}`)");
                    else
                        messageBuilder.AppendLine($"**{maintainer.CachedName}** (`{maintainer.DiscordUserId}`)");
                });
                await ic.CreateResponseAsync(messageBuilder.ToString());
            }
        }

    }
}
