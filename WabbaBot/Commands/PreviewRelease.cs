using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(PreviewRelease), "Preview your release message before actually sending it out")]
        public async Task PreviewRelease(InteractionContext ic, [Option("Modlist", "The modlist you want to send out release notifications for", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL, [Option("Message", @"The release message you want to send out. Markdown supported, use \n for a new line.")] string message) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == null) {
                    await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} is not being managed by WabbaBot!"));
                    return;
                }
                else {
                    await Bot.ReloadModlistsAsync();
                    var modlistMetadata = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == machineURL);
                    if (modlistMetadata == null) {
                        await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} was not found externally!"));
                        return;
                    }

                    DiscordEmbed embed = await GetReleaseEmbedForModlist(ic, message, modlistMetadata);
                    await ic.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
                }
            }
        }

    }
}
