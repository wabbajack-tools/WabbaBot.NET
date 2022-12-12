using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Text;
using Wabbajack.DTOs;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [SlashCommand(nameof(ShowManagedModlists), "Shows the modlists WabbaBot is managing, you can subscribe to these.")]
        public async Task ShowManagedModlists(InteractionContext ic) {
            using (var dbContext = new BotDbContext()) {
                var managedModlists = dbContext.ManagedModlists;
                if (!managedModlists.Any())
                    await ic.CreateResponseAsync("Uh oh! Looks like there are no modlists managed by WabbaBot yet. Soon:tm:!");
                else {
                    await Bot.ReloadModlistsAsync();
                    StringBuilder messageBuilder = new StringBuilder();
                    int i = 1;
                    foreach (var managedModlist in managedModlists) {
                        var modlist = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == managedModlist.MachineURL);
                        if (modlist != default(ModlistMetadata))
                            messageBuilder.AppendLine($"{i} - **{modlist.Title}** (`{modlist.Links.MachineURL}`) made by {modlist.Author}.");
                        else
                            messageBuilder.AppendLine($"{i} - **{managedModlist.MachineURL}** maintained by {managedModlist.Maintainers.Select(maintainer => maintainer.CachedName).CreateJoinedString(", ", " and ")}");
                        i++;
                    }
                    var interactivity = ic.Client.GetInteractivity();
                    var pages = interactivity.GeneratePagesInEmbed(messageBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line);
                    await interactivity.SendPaginatedResponseAsync(ic.Interaction, true, ic.Member, pages);
                }
            }
        }

    }
}
