using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WabbaBot.Helpers;
using WabbaBot.Interfaces;

namespace WabbaBot.ModalResponses {
    public class PreviewReleaseModalResponse : IModalResponse {
        public string ResponseId => nameof(Commands.SlashCommands.PreviewRelease);
        public async Task Respond(DiscordClient client, ModalSubmitEventArgs e, string? machineURL) {
            await e.Interaction.DeferAsync(true);
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == null) {
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} is not being managed by WabbaBot!"));
                    return;
                }
                else {
                    await Bot.ReloadModlistsAsync();
                    var modlistMetadata = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == machineURL);
                    if (modlistMetadata == null) {
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} was not found externally!"));
                        return;
                    }

                    DiscordEmbed embed = await DiscordHelper.GetReleaseEmbedForModlist(e.Interaction, e.Values["message"], modlistMetadata, e.Values["version"]);
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
        }
    }
}
