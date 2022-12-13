using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Helpers;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(PreviewRelease), "Preview your release message before actually sending it out")]
        public async Task PreviewRelease(InteractionContext ic, [Option("Modlist", "The modlist you want to send out release notifications for", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string? machineURL) {
            var modlist = Bot.Modlists.FirstOrDefault(m => m.Links.MachineURL == machineURL);
            if (modlist == null) {
                ic.Client.Logger.LogError($"Modlist with id {machineURL} not found (previewrelease).");
                return;
            }
            var title = $"Preview releasing {modlist.Title} v{modlist.Version}";

            // Some shitty Discord limit, modal won't show otherwise >:(
            if (title.Length > 45)
                title = "Preview releasing your modlist";

            var response = new DiscordInteractionResponseBuilder();
            response.WithTitle(title)
                    .WithCustomId($"{nameof(PreviewRelease)}|{machineURL}")
                    .AddComponents(new TextInputComponent(label: "Release message", customId: "message", placeholder: "Updated 8K Mammoth Tusks to 1.3.3.7", style: TextInputStyle.Paragraph));
            await ic.CreateResponseAsync(InteractionResponseType.Modal, response);
        }

    }
}
