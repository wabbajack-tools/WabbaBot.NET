﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(SetTemplate), "Set a template to suggest when releasing this modlist")]
        public async Task SetTemplate(InteractionContext ic, [Option("Modlist", "The modlist to set a template for"), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL) {
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
                    var response = new DiscordInteractionResponseBuilder();
                    response.WithTitle("Set the template for your modlist")
                            .WithCustomId($"{nameof(SetTemplate)}|{machineURL}")
                            .AddComponents(new TextInputComponent(label: "Template", customId: "template", placeholder: "Enter the release template, leave empty to clear it.", style: TextInputStyle.Paragraph, value: string.IsNullOrEmpty(releaseTemplate?.Content) ? null : releaseTemplate.Content));
                    await ic.CreateResponseAsync(InteractionResponseType.Modal, response);

                }
            }
            catch (Exception ex) {
                await ic.CreateResponseAsync($"Failed to set a template for this modlist. {ex.Message}");
            }
        }
    }
}
