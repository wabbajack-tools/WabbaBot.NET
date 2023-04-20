using DSharpPlus.SlashCommands;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(SetTemplate), "Set a template to suggest when releasing this modlist")]
        public async Task SetTemplate(InteractionContext ic, [Option("Modlist", "The modlist to set a template for"), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Template", "The template that should be suggested when releasing this modlist (leave blank to remove)")] string? templateContent = null) {
            try {
                using (var dbContext = new BotDbContext()) {
                    var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);

                    if (managedModlist == default) {
                        await ic.CreateResponseAsync($"Could not find modlist with machineURL **{machineURL}** to set a template for!");
                        return;
                    }

                    var releaseTemplate = dbContext.ReleaseTemplates.FirstOrDefault(rt => rt.ManagedModlistId == managedModlist.Id);
                    await Bot.ReloadModlistsAsync();
                    var managedModlistMetadata = Bot.Modlists.FirstOrDefault(mmm => mmm.Links.MachineURL == machineURL);
                    if (managedModlistMetadata == null) {
                        await ic.CreateResponseAsync($"Modlist with machineURL **{machineURL}** does not exist in an external repository!");
                        return;
                    }

                    if (templateContent == null) {
                        if (releaseTemplate == null) {
                            await ic.CreateResponseAsync($"Modlist **{managedModlistMetadata.Title}** doesn't have a template, so it can't be removed.");
                            return;
                        }
                        dbContext.ReleaseTemplates.Remove(releaseTemplate);
                    }
                    else {
                        ReleaseTemplate rt = new ReleaseTemplate() {
                            Content = templateContent,
                            ManagedModlistId = managedModlist.Id
                        };
                        dbContext.ReleaseTemplates.Add(rt);
                    }
                    dbContext.SaveChanges();
                    if (templateContent == null)
                        await ic.CreateResponseAsync($"Modlist **{managedModlistMetadata.Title}** no longer has a template.");
                    else
                        await ic.CreateResponseAsync($"Modlist **{managedModlistMetadata.Title}** now has a template set!");
                }
            }
            catch (Exception ex) {
                await ic.CreateResponseAsync($"Failed to set a template for this modlist. {ex.Message}");
            }
        }
    }
}
