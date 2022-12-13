using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [SlashRequireUserPermissions(Permissions.ManageRoles)]
        [SlashCommand(nameof(ClearRole), "Remove the ping role for the specified modlist")]
        public async Task ClearRole(InteractionContext ic, [Option("Modlist", "The modlist to receive mentions/pings for on release notifications", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist {machineURL} is not managed by WabbaBot!");
                    return;
                }

                var role = dbContext.PingRoles.FirstOrDefault(pr => pr.ManagedModlistId == managedModlist.Id && pr.DiscordGuildId == ic.Guild.Id);
                if (role != default(PingRole)) {
                    dbContext.PingRoles.Remove(role);
                }
                else {
                    await ic.CreateResponseAsync("No ping role was set for that modlist!");
                }

                dbContext.SaveChanges();
                await ic.CreateResponseAsync($"Release notifications for {machineURL} will no longer ping any role.");
            }
        }
    }
}
