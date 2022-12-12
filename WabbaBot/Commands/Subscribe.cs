using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [SlashRequireUserPermissions(Permissions.ManageRoles)]
        [RequireMentionedChannelMessagingPermissions]
        [SlashCommand(nameof(Subscribe), "Subscribe to a modlist in a specific channel")]
        public async Task Subscribe(InteractionContext ic, [Option("Modlist", "The modlist you want to subscribe to", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Channel", "The channel you want the release notifications for this modlist to appear in")] DiscordChannel discordChannel) {
            using (var dbContext = new BotDbContext()) {
                if (discordChannel.IsCategory || discordChannel.IsThread) {
                    await ic.CreateResponseAsync($"How am I going to send out release notifications there? Please specify a specific channel.");
                    return;
                }
                var subscribedChannel = dbContext.SubscribedChannels.FirstOrDefault(sc => sc.DiscordChannelId == discordChannel.Id);
                await Bot.ReloadModlistsAsync();
                var modlistMetadata = Bot.Modlists.FirstOrDefault(mm => mm.Links.MachineURL == machineURL);
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == null) {
                    await ic.CreateResponseAsync($"Modlist with machineURL **{machineURL}** is not being managed by WabbaBot.");
                    return;
                }
                if (subscribedChannel != null) {
                    dbContext.Entry(subscribedChannel).Collection(lm => lm.ManagedModlists).Load();
                    if (subscribedChannel.ManagedModlists.Any(m => m.MachineURL == machineURL)) {
                        await ic.CreateResponseAsync($"This channel is already subscribed to **{modlistMetadata?.Title ?? machineURL}**!");
                        return;
                    }
                }
                else {
                    subscribedChannel = new SubscribedChannel(discordChannel.Id, discordChannel.Guild.Id, discordChannel.Name);
                    dbContext.SubscribedChannels.Add(subscribedChannel);
                }
                subscribedChannel.ManagedModlists.Add(managedModlist);
                dbContext.SaveChanges();
                await ic.CreateResponseAsync($"Now subscribed to **{modlistMetadata?.Title ?? machineURL}** in {discordChannel.Mention}.");
            }
        }

    }
}
