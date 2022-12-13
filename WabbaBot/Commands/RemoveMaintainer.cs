using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(RemoveMaintainer), "Remove permissions to manage modlist releases for a maintainer of the specified list")]
        public async Task RemoveMaintainer(InteractionContext ic, [Option("Modlist", "The modlist to remove a maintainer from", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Maintainer", "The person that should no longer be able to manage modlist releases for the selected list")] DiscordUser discordUser) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(lm => lm.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist **{machineURL}** does not exist!");
                    return;
                }

                dbContext.Entry(managedModlist).Collection(lm => lm.Maintainers).Load();
                var maintainer = managedModlist.Maintainers.FirstOrDefault(m => m.DiscordUserId == discordUser.Id);
                if (maintainer == default(Maintainer)) {
                    await ic.CreateResponseAsync($"{discordUser.Username} is not maintaining **{machineURL}**!");
                    return;
                }
                else {
                    managedModlist.Maintainers.Remove(maintainer);
                    if (!managedModlist.Maintainers.Any())
                        dbContext.ManagedModlists.Remove(managedModlist);
                    dbContext.SaveChanges();
                    await ic.CreateResponseAsync($"{discordUser.Username} is no longer maintaining **{machineURL}**.");
                }
            }
        }


    }
}
