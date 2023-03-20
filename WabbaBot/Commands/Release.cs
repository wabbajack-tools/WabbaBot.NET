using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(Release), "Release one of your maintained modlists")]
        public async Task Release(InteractionContext ic, [Option("Modlist", "The modlist you want to send out release notifications for", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL) {
            var modlist = Bot.Modlists.FirstOrDefault(m => m.Links.MachineURL == machineURL);
            if (modlist == null) {
                ic.Client.Logger.LogError($"Modlist with id {machineURL} not found (release).");
                return;
            }
            var title = $"Releasing {modlist.Title} v{modlist.Version}";

            // Some shitty Discord limit, modal won't show otherwise >:(
            if (title.Length > 45)
                title = "Release your modlist";

            var response = new DiscordInteractionResponseBuilder();
            response.WithTitle(title)
                    .WithCustomId($"{nameof(Release)}|{machineURL}")
                    .AddComponents(new TextInputComponent(label: "Release message", customId: "message", placeholder: "Updated 8K Mammoth Tusks to 1.3.3.7", style: TextInputStyle.Paragraph))
//                    .AddComponents(new TextInputComponent(label: "Version", customId: "version", placeholder: $"{modlist.Version}"));
            await ic.CreateResponseAsync(InteractionResponseType.Modal, response);
        }

    }
}
