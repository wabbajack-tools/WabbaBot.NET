﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text;
using Wabbajack.DTOs;
using WabbaBot.Core;
using DSharpPlus.Interactivity.Extensions;
using WabbaBot.Models;
using Microsoft.EntityFrameworkCore;
using WabbaBot.Core.EqualityComparers;
using WabbaBot.Commands.Attributes;
using WabbaBot.Commands.AutocompleteProviders;
using WabbaBot.Core.Models;

namespace WabbaBot.Commands {
    public class SlashCommands : ApplicationCommandModule {
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
            await ic.Channel.SendPaginatedMessageAsync(ic.Member, pages);
        }

        [SlashCommand(nameof(ShowManagedModlists), "Shows the modlists WabbaBot is managing, you can subscribe to these.")]
        public async Task ShowManagedModlists(InteractionContext ic) {
            using (var dbContext = new BotDbContext()) {
                var managedModlists = dbContext.ManagedModlists;
                if (!managedModlists.Any())
                    await ic.CreateResponseAsync("Uh oh! Looks like there are no modlists managed by WabbaBot. Add some maintainers to one of the external lists to get going!");
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
                    await ic.Channel.SendPaginatedMessageAsync(ic.Member, pages);
                }
            }
        }

        [MaintainersOnly]
        [SlashCommand(nameof(AddMaintainer), "Give someone permissions to manage release notifications for a modlist.")]
        public async Task AddMaintainer(InteractionContext ic, [Option("Modlist", "The modlist to add a maintainer to"), Autocomplete(typeof(ExternalModlistsAutocompleteProvider))] string machineURL, [Option("Maintainer", "The person that should be able to manage modlist releases for the selected list")] DiscordUser discordUser) {
            if (discordUser.IsBot) {
                await ic.CreateResponseAsync($"**{discordUser.Username}** cannot maintain a modlist, it's a bot!");
                return;
            }

            await Bot.ReloadModlistsAsync(forceReload: true);
            var modlistMetadata = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == machineURL);
            if (modlistMetadata == null) {
                await ic.CreateResponseAsync($"Modlist with id **{machineURL}** does not exist externally.");
                return;
            }

            using (var dbContext = new BotDbContext()) {
                var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == discordUser.Id);
                var managedModlist = dbContext.ManagedModlists.Include(lm => lm.Maintainers).FirstOrDefault(lm => lm.MachineURL == machineURL);
                if (managedModlist != default(ManagedModlist) && managedModlist.Maintainers.Exists(m => m.DiscordUserId == discordUser.Id)) {
                    await ic.CreateResponseAsync($"**{discordUser.Username}** is already maintaining **{modlistMetadata.Title}**!");
                    return;
                }
                if (maintainer == default(Maintainer)) {
                    maintainer = new Maintainer(discordUser.Id, discordUser.Username);
                    dbContext.Maintainers.Add(maintainer);
                }
                if (managedModlist == default(ManagedModlist)) {
                    managedModlist = new ManagedModlist(machineURL);
                    dbContext.ManagedModlists.Add(managedModlist);
                }
                managedModlist.Maintainers.Add(maintainer);
                dbContext.SaveChanges();
            }
            await ic.CreateResponseAsync($"**{discordUser.Username}** is now maintaining **{modlistMetadata.Title}**.");
        }

        [MaintainersOnly]
        [SlashCommand(nameof(RemoveMaintainer), "Remove permissions to manage modlist releases for a maintainer of the specified list")]
        public async Task RemoveMaintainer(InteractionContext ic, [Option("Modlist", "The modlist to remove a maintainer from", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Maintainer", "The person that should no longer be able to manage modlist releases for the selected list")] DiscordUser discordUser) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(lm => lm.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist **{machineURL}** does not exist!");
                    return;
                }

                dbContext.Entry(managedModlist).Collection(lm => lm.Maintainers).Load();
                var maintainer = managedModlist.Maintainers.FirstOrDefault(m => m.DiscordUserId == discordUser.Id);
                if (maintainer == default(Maintainer)) {
                    await ic.CreateResponseAsync($"{discordUser.Username} is not maintaining **{machineURL}**!");
                    return;
                }
                else {
                    managedModlist.Maintainers.Remove(maintainer);
                    if (!managedModlist.Maintainers.Any())
                        dbContext.ManagedModlists.Remove(managedModlist);
                    dbContext.SaveChanges();
                    await ic.CreateResponseAsync($"{discordUser.Username} is no longer maintaining **{machineURL}**.");
                }
            }
        }

        [MaintainersOnly]
        [SlashCommand(nameof(ShowMaintainers), "Show everyone maintaining a specific modlist")]
        public async Task ShowMaintainers(InteractionContext ic, [Option("Modlist", "The modlist you want to show the maintainers for", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.Include(m => m.Maintainers).FirstOrDefault(m => m.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist with machineURL **{machineURL}** not found.");
                    return;
                }
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"Modlist **{machineURL}** is being maintained by {managedModlist.Maintainers.Count} maintainer(s): ");
                Parallel.ForEach(managedModlist.Maintainers, maintainer => {
                    var discordUser = ic.Client.GetUserAsync(maintainer.DiscordUserId).Result;
                    if (discordUser != null)
                        messageBuilder.AppendLine($"**{discordUser.Username}#{discordUser.Discriminator}** (`{discordUser.Id}`)");
                    else
                        messageBuilder.AppendLine($"**{maintainer.CachedName}** (`{maintainer.DiscordUserId}`)");
                });
                await ic.CreateResponseAsync(messageBuilder.ToString());
            }
        }

        [MaintainersOnly]
        [SlashCommand(nameof(Release), "Release one of your maintained modlists")]
        public async Task Release(InteractionContext ic, [Option("Modlist", "The modlist you want to send out release notifications for", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL, [Option("Message", "The release message you want to send out. Markdown supported!"), RemainingText] string message) {
            await ic.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == null) {
                    await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} is not being managed by WabbaBot!"));
                    return;
                }
                else {
                    await Bot.ReloadModlistsAsync();
                    var modlistMetadata = Bot.Modlists.Find(modlist => modlist.Links.MachineURL == machineURL);
                    if (modlistMetadata == null) {
                        await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist with machineURL {machineURL} was not found externally!"));
                        return;
                    }

                    DiscordEmbed embed = new DiscordEmbedBuilder() {
                        Title = $"{ic.User.Username} just released {modlistMetadata.Title} v{modlistMetadata.Version}!",
                        Timestamp = DateTime.Now,
                        Description = message,
                        ImageUrl = modlistMetadata.Links.ImageUri,
                        Footer = new DiscordEmbedBuilder.EmbedFooter() {
                            Text = "WabbaBot"
                        }
                    }.Build();

                    dbContext.Entry(managedModlist).Collection(lm => lm.SubscribedChannels).Load();
                    if (managedModlist.SubscribedChannels.Any()) {
                        var pingRole = dbContext.PingRoles.FirstOrDefault(pr => pr.ManagedModlistId == managedModlist.Id && pr.DiscordGuildId == ic.Guild.Id);

                        var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == ic.User.Id);
                        if (maintainer == default(Maintainer)) {
                            await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Maintainer not found. This error should never appear in the first place, what's going on?!"));
                            return;
                        }


                        var release = new Release(managedModlist.Id);
                        var dbGroup = dbContext.Releases.Add(release);
                        dbContext.SaveChanges();

                        bool anyReleased = false;
                        foreach (var subscribedChannel in managedModlist.SubscribedChannels) {
                            var discordChannel = await ic.Client.GetChannelAsync(subscribedChannel.DiscordChannelId);
                            var discordMessage = await discordChannel.SendMessageAsync(embed);
                            if (discordMessage != default(DiscordMessage)) {
                                var releaseMessage = new ReleaseMessage(message, discordMessage.Id, managedModlist.Id, subscribedChannel.Id, maintainer.Id, dbGroup.Entity.Id);
                                if (pingRole != default(PingRole)) {
                                    await discordChannel.SendMessageAsync(ic.Guild.GetRole(pingRole.DiscordRoleId).Mention);
                                }
                                dbContext.ReleaseMessages.Add(releaseMessage);
                                anyReleased = true;
                            }
                        }
                        if (!anyReleased)
                            dbContext.Releases.Remove(release);

                        dbContext.SaveChanges();
                        await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist was released in {managedModlist.SubscribedChannels.Count} channel(s) across {managedModlist.SubscribedChannels.GroupBy(sc => sc.DiscordGuildId).Count()} server(s)!"));
                    }
                    else {
                        await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"No channels are subscribed to **{modlistMetadata.Title}**!"));
                    }
                }
            }
        }

        [MaintainersOnly]
        [SlashCommand(nameof(Revise), "Revise one of the release messages for the specified list")]
        public async Task Revise(InteractionContext ic, [Option("Modlist", "The modlist you want to revise a release message of", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL, [Option("Message", "The release message you want to send out. Markdown supported!"), RemainingText] string message) {
            using (var dbContext = new BotDbContext()) {
                var latestRelease = dbContext.Releases.Include(rmg => rmg.ManagedModlist).OrderByDescending(rmg => rmg.CreatedOn).FirstOrDefault(rmg => rmg.ManagedModlist.MachineURL == machineURL);
                if (latestRelease == default(Release)) {
                    var managedModlist = dbContext.ManagedModlists.FirstOrDefault(managedModlist => managedModlist.MachineURL == machineURL);
                    if (managedModlist == default(ManagedModlist)) {
                        await ic.CreateResponseAsync("This modlist either doesn't exist or it isn't maintained by anybody!");
                    }
                    await ic.CreateResponseAsync($"No releases found for modlist **{machineURL}**!");
                }
                dbContext.Entry(latestRelease).Collection(r => r.ReleaseMessages).Load();

                bool edited = false;
                foreach(var releaseMessage in latestRelease.ReleaseMessages) {
                    dbContext.Entry(releaseMessage).Reference(rm => rm.SubscribedChannel).Load();
                    var discordGuild = await ic.Client.GetGuildAsync(releaseMessage.SubscribedChannel.DiscordGuildId);
                    var discordChannel = discordGuild.GetChannel(releaseMessage.SubscribedChannel.DiscordChannelId);
                    var discordMessage = await discordChannel.GetMessageAsync(releaseMessage.DiscordMessageId);
                    var newEmbed = new DiscordEmbedBuilder(discordMessage.Embeds.First());
                    newEmbed.Description = message;
                    var editedMessage = await discordMessage.ModifyAsync(newEmbed.Build());
                    if (editedMessage != default(DiscordMessage)) {
                        releaseMessage.Message = message;
                        edited = true;
                    }
                }
                if (edited) {
                    dbContext.SaveChanges();
                    await ic.CreateResponseAsync($"Succesfully revised {latestRelease.ReleaseMessages.Count} messages.");
                }
            }
        }

        [RequirePermissions(Permissions.ManageRoles)]
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

        [RequirePermissions(Permissions.ManageRoles)]
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

        [BotAdministratorsOnly]
        [SlashCommand(nameof(ShowAllSubscriptions), "Show all servers and channels that are subscribed to the specified modlist (bot admin only)")]
        public async Task ShowAllSubscriptions(InteractionContext ic, [Option("Modlist", "The modlist you want to see all the subscriptions for", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(m => m.MachineURL == machineURL);
                if (managedModlist == null) {
                    await ic.CreateResponseAsync($"Modlist with machineURL **{machineURL}** is not being managed by WabbaBot.");
                    return;
                }
                await Bot.ReloadModlistsAsync();
                var modlistMetadata = Bot.Modlists.FirstOrDefault(m => m.Links.MachineURL == machineURL);
                dbContext.Entry(managedModlist).Collection(mm => mm.SubscribedChannels);
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"{modlistMetadata?.Title ?? managedModlist.MachineURL} has {managedModlist.SubscribedChannels.Count} subscription(s).");
                var orderedChannels = managedModlist.SubscribedChannels.OrderBy(sc => sc.DiscordGuildId);
                SubscribedChannel? previousChannel = null;
                foreach (var subscribedChannel in orderedChannels) {
                    var discordChannel = await ic.Client.GetChannelAsync(subscribedChannel.DiscordChannelId);
                    if (previousChannel == null || subscribedChannel.DiscordGuildId != previousChannel.DiscordGuildId) {
                        messageBuilder.AppendLine($"Server **{discordChannel.Guild.Name}** is listening to **${modlistMetadata?.Title ?? managedModlist.MachineURL}** in the following channels:");
                    }
                    messageBuilder.AppendLine($"- **{discordChannel.Name}** (`{discordChannel.Id}`)");
                    previousChannel = subscribedChannel;
                }
                await ic.CreateResponseAsync(messageBuilder.ToString());
            }
        }

        [MaintainersOnly]
        [SlashCommand(nameof(ShowSubscriptions), "Show all modlists that are subscribed to a channel in this server")]
        public async Task ShowSubscriptions(InteractionContext ic) {
            using (var dbContext = new BotDbContext()) {
                var subscribedChannels = dbContext.SubscribedChannels.Include(sc => sc.ManagedModlists).Where(sc => sc.DiscordGuildId == ic.Guild.Id);
                if (!subscribedChannels.Any()) {
                    await ic.CreateResponseAsync($"This server isn't subscribed to any modlists!");
                    return;
                }
                StringBuilder messageBuilder = new StringBuilder();
                foreach (var subscribedChannel in subscribedChannels) {
                    var discordChannel = ic.Guild.GetChannel(subscribedChannel.DiscordChannelId);
                    messageBuilder.AppendLine($"{discordChannel.Mention} is subscribed to **{subscribedChannel.ManagedModlists.Select(mm => mm.MachineURL).CreateJoinedString("**, **", "** and **")}**.");
                }
                await ic.CreateResponseAsync(messageBuilder.ToString());
            }
        }

        [RequirePermissions(Permissions.ManageRoles)]
        [SlashCommand(nameof(SetRole), "Set a role to mention/ping whenever the specified modlist is released")]
        public async Task SetRole(InteractionContext ic, [Option("Modlist", "The modlist to receive mentions/pings for on release notifications", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Role", "The role that should be mentioned/pinged when the specified modlist releases")] DiscordRole discordRole) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist {machineURL} is not managed by WabbaBot!");
                    return;
                }
                dbContext.Entry(managedModlist).Collection(lm => lm.SubscribedChannels).Load();
                if (!managedModlist.SubscribedChannels.Any(sc => sc.DiscordGuildId == ic.Guild.Id)) {
                    await ic.CreateResponseAsync($"No channels are subscribed to {machineURL}! Please subscribe to the modlist prior to setting a mention/ping role.");
                    return;
                }

                var role = dbContext.PingRoles.FirstOrDefault(pr => pr.DiscordRoleId == discordRole.Id);
                if (role == default(PingRole)) {
                    role = new PingRole(discordRole.Id, ic.Guild.Id, managedModlist.Id);
                    dbContext.PingRoles.Add(role);
                }
                else {
                    role.ManagedModlistId = managedModlist.Id;
                    dbContext.Entry(role).State = EntityState.Modified;
                }

                dbContext.SaveChanges();
                await ic.CreateResponseAsync($"Release notifications for {machineURL} will now ping the **{discordRole.Name}** role.");
            }
        }
    }
}
