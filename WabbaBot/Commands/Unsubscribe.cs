using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot.Commands {
    public partial class SlashCommands : ApplicationCommandModule {
        [SlashRequireUserPermissions(Permissions.ManageRoles)]
        [SlashCommand(nameof(Unsubscribe), "Unsubscribe from a modlist in a specific channel")]
        public async Task Unsubscribe(InteractionContext ic, [Option("Modlist", "The modlist you want to unsubscribe from", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Channel", "The channel you want the release notifications for this modlist to appear in")] DiscordChannel discordChannel) {
            using (var dbContext = new BotDbContext()) {
                var subscribedChannel = dbContext.SubscribedChannels.FirstOrDefault(sc => sc.DiscordChannelId == discordChannel.Id);
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist with machineURL **{machineURL}** is not being managed by WabbaBot.");
                    return;
                }
                await Bot.ReloadModlistsAsync();
                var modlistMetadata = Bot.Modlists.FirstOrDefault(mm => mm.Links.MachineURL == machineURL);

                if (subscribedChannel != null) {
                    dbContext.Entry(subscribedChannel).Collection(sc => sc.ManagedModlists).Load();
                    if (subscribedChannel.ManagedModlists.Remove(managedModlist)) {
                        dbContext.SaveChanges();
                        await ic.CreateResponseAsync($"No longer subscribed to **{modlistMetadata?.Title ?? machineURL}** in {discordChannel.Mention}.");
                        return;
                    }
                }
                await ic.CreateResponseAsync($"This channel isn't subscribed to **{modlistMetadata?.Title ?? machineURL}**!");
            }
        }

    }
}
