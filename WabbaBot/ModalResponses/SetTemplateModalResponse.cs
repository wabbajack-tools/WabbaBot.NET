using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WabbaBot.Interfaces;
using DSharpPlus.Entities;
using WabbaBot.Models;

namespace WabbaBot.ModalResponses {
    public class SetTemplateModalResponse : IModalResponse {
        public string ResponseId => nameof(Commands.SlashCommands.SetTemplate);
        public async Task Respond(DiscordClient client, ModalSubmitEventArgs e, string? machineURL) {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var templateContent = e.Values["template"];
            using (var dbContext = new BotDbContext()) {

                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == default) {
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} is not being managed by WabbaBot!"));
                    return;
                }
                var modlist = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == machineURL);

                var releaseTemplate = dbContext.ReleaseTemplates.FirstOrDefault(rt => rt.ManagedModlistId == managedModlist.Id);

                if (string.IsNullOrEmpty(templateContent)) {
                    if (releaseTemplate == null) {
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist **{modlist?.Title}** doesn't have a template, so it can't be removed."));
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
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist **{modlist?.Title}** no longer has a template set."));
                else
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist **{modlist?.Title}** now has a template set!"));
            }
        }
    }
}
