using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

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
            ReleaseTemplate template = null;
            using(var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.Include(mm => mm.ReleaseTemplate).FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist != null)
                    template = managedModlist.ReleaseTemplate;
            }

            // Some shitty Discord limit, modal won't show otherwise >:(
            if (title.Length > 45)
                title = "Release your modlist";

            var response = new DiscordInteractionResponseBuilder();
            response.WithTitle(title)
                    .WithCustomId($"{nameof(Release)}|{machineURL}")
                    .AddComponents(new TextInputComponent(label: "Release message", customId: "message", placeholder: "Set a template here using /settemplate!", style: TextInputStyle.Paragraph, value: template != null ? template.Content : null));
//                    .AddComponents(new TextInputComponent(label: "Version", customId: "version", placeholder: $"{modlist.Version}"));
            await ic.CreateResponseAsync(InteractionResponseType.Modal, response);
        }

    }
}
