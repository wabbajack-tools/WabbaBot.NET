using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Text;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(ShowAllSubscriptions), "Show all servers and channels that are subscribed to the specified modlist (bot admin only)")]
        public async Task ShowAllSubscriptions(InteractionContext ic, [Option("Modlist", "The modlist you want to see all the subscriptions for", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL) {
            await ic.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(m => m.MachineURL == machineURL);
                if (managedModlist == null) {
                    await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL **{machineURL}** is not being managed by WabbaBot."));
                    return;
                }
                await Bot.ReloadModlistsAsync();
                var modlistMetadata = Bot.Modlists.FirstOrDefault(m => m.Links.MachineURL == machineURL);
                dbContext.Entry(managedModlist).Collection(mm => mm.SubscribedChannels).Load();
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"{modlistMetadata?.Title ?? managedModlist.MachineURL} has {managedModlist.SubscribedChannels.Count} subscription(s).");
                var orderedChannels = managedModlist.SubscribedChannels.OrderBy(sc => sc.DiscordGuildId);
                SubscribedChannel? previousChannel = null;
                foreach (var subscribedChannel in orderedChannels) {
                    try {
                        var discordChannel = await ic.Client.GetChannelAsync(subscribedChannel.DiscordChannelId);
                        if (previousChannel == null || subscribedChannel.DiscordGuildId != previousChannel.DiscordGuildId) {
                            messageBuilder.AppendLine($"Server **{discordChannel.Guild.Name}** is subscribed to **{modlistMetadata?.Title ?? managedModlist.MachineURL}** in the following channels:");
                        }
                        messageBuilder.AppendLine($"- **{discordChannel.Name}** (`{discordChannel.Id}`)");
                    }
                    catch { 
                        continue;
                    }
                    previousChannel = subscribedChannel;
                }
                var interactivity = ic.Client.GetInteractivity();
                var pages = interactivity.GeneratePagesInEmbed(messageBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line);
                await ic.Interaction.SendPaginatedResponseAsync(true, ic.Member, pages, asEditResponse: true);
            }
        }


    }
}
