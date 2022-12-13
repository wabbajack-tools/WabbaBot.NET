using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(AddMaintainer), "Give someone permissions to manage release notifications for a modlist.")]
        public async Task AddMaintainer(InteractionContext ic, [Option("Modlist", "The modlist to add a maintainer to"), Autocomplete(typeof(ExternalModlistsAutocompleteProvider))] string machineURL, [Option("Maintainer", "The person that should be able to manage modlist releases for the selected list")] DiscordUser discordUser) {
            if (discordUser.IsBot) {
                await ic.CreateResponseAsync($"**{discordUser.Username}** cannot maintain a modlist, it's a bot!");
                return;
            }

            await Bot.ReloadModlistsAsync(forceReload: true);
            var modlistMetadata = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == machineURL);
            if (modlistMetadata == null) {
                await ic.CreateResponseAsync($"Modlist with id **{machineURL}** does not exist externally.");
                return;
            }

            using (var dbContext = new BotDbContext()) {
                var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == discordUser.Id);
                var managedModlist = dbContext.ManagedModlists.Include(lm => lm.Maintainers).FirstOrDefault(lm => lm.MachineURL == machineURL);
                if (managedModlist != default(ManagedModlist) && managedModlist.Maintainers.Exists(m => m.DiscordUserId == discordUser.Id)) {
                    await ic.CreateResponseAsync($"**{discordUser.Username}** is already maintaining **{modlistMetadata.Title}**!");
                    return;
                }
                if (maintainer == default(Maintainer)) {
                    maintainer = new Maintainer(discordUser.Id, discordUser.Username);
                    dbContext.Maintainers.Add(maintainer);
                }
                if (managedModlist == default(ManagedModlist)) {
                    managedModlist = new ManagedModlist(machineURL);
                    dbContext.ManagedModlists.Add(managedModlist);
                }
                managedModlist.Maintainers.Add(maintainer);
                dbContext.SaveChanges();
            }
            await ic.CreateResponseAsync($"**{discordUser.Username}** is now maintaining **{modlistMetadata.Title}**.");
        }

    }
}
