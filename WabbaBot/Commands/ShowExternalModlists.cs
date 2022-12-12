using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Text;
using Wabbajack.DTOs;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [SlashCommand(nameof(ShowExternalModlists), "Shows all the external modlists that can be managed by WabbaBot.")]
        public async Task ShowExternalModlists(InteractionContext ic) {
            await Bot.ReloadModlistsAsync();
            StringBuilder messageBuilder = new StringBuilder();
            int i = 1;
            foreach (var modlist in Bot.Modlists) {
                if (modlist != default(ModlistMetadata))
                    messageBuilder.AppendLine($"{i} - **{modlist.Title}** (`{modlist.Links.MachineURL}`) made by {modlist.Author}.");
                i++;
            }
            var interactivity = ic.Client.GetInteractivity();
            var pages = interactivity.GeneratePagesInEmbed(messageBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line);
            await interactivity.SendPaginatedResponseAsync(ic.Interaction, true, ic.Member, pages);
        }

    }
}
