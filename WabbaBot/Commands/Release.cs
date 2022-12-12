using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Helpers;
using WabbaBot.Models;
using Wabbajack.DTOs;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        private static async Task<DiscordEmbed> GetReleaseEmbedForModlist(InteractionContext ic, string message, ModlistMetadata modlistMetadata) {
            var color = await ImageProcessing.GetColorFromImageUrlAsync(modlistMetadata.Links.ImageUri);
            DiscordEmbed embed = new DiscordEmbedBuilder() {
                Title = $"{ic.User.Username} just released {modlistMetadata.Title} v{modlistMetadata.Version}!",
                Timestamp = DateTime.Now,
                Description = message.Replace(@"\n", "\n"),
                ImageUrl = modlistMetadata.Links.ImageUri,
                Color = new DiscordColor(color.ToHex().Remove(6, 2)),
                Footer = new DiscordEmbedBuilder.EmbedFooter() {
                    Text = "WabbaBot"
                }
            }.Build();

            return embed;
        }

        [RequireModlistMaintainer]
        [SlashCommand(nameof(Release), "Release one of your maintained modlists")]
        public async Task Release(InteractionContext ic, [Option("Modlist", "The modlist you want to send out release notifications for", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL, [Option("Message", @"The release message you want to send out. Markdown supported, use \n for a new line.")] string message) {
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

                    DiscordEmbed embed = await GetReleaseEmbedForModlist(ic, message, modlistMetadata);

                    dbContext.Entry(managedModlist).Collection(lm => lm.SubscribedChannels).Load();
                    if (managedModlist.SubscribedChannels.Any()) {

                        var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == ic.User.Id);
                        if (maintainer == default(Maintainer)) {
                            await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Maintainer not found. This error should never appear in the first place, what's going on?!"));
                            return;
                        }


                        var release = new Release(managedModlist.Id);
                        var dbGroup = dbContext.Releases.Add(release);
                        dbContext.SaveChanges();

                        int releaseAmount = 0;
                        foreach (var subscribedChannel in managedModlist.SubscribedChannels) {
                            try {
                                var discordChannel = await ic.Client.GetChannelAsync(subscribedChannel.DiscordChannelId);
                                var discordMessage = await discordChannel.SendMessageAsync(embed);
                                if (discordMessage != default) {
                                    var releaseMessage = new ReleaseMessage(message, discordMessage.Id, managedModlist.Id, subscribedChannel.Id, maintainer.Id, dbGroup.Entity.Id);
                                    var pingRole = dbContext.PingRoles.FirstOrDefault(pr => pr.ManagedModlistId == managedModlist.Id && pr.DiscordGuildId == discordChannel.Guild.Id);
                                    if (pingRole != default(PingRole)) {
                                        try {
                                            var role = ic.Guild.GetRole(pingRole.DiscordRoleId);
                                            await discordChannel.SendMessageAsync(role.Mention);
                                        }
                                        catch (Exception ex) {
                                            await discordChannel.SendMessageAsync("I wanted to ping a role here, but it seems like it no longer exists! Set a new one with /setrole. Alternatively, use /clearrole to confirm you don't want any roles to be pinged when this modlist releases, and to prevent this message from showing up again.");
                                            ic.Client.Logger.LogError($"Role with database id {pingRole.Id} could not be pinged! Exception: {ex.Message}\n{ex.StackTrace}");
                                        }
                                    }
                                    dbContext.ReleaseMessages.Add(releaseMessage);
                                    releaseAmount++;
                                }
                            }
                            catch (Exception ex) {
                                ic.Client.Logger.LogError($"Could not release a message for {modlistMetadata.Title} in {subscribedChannel.CachedName} ({subscribedChannel.DiscordChannelId}) with guild id {subscribedChannel.DiscordGuildId}. Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                        if (releaseAmount == 0)
                            dbContext.Releases.Remove(release);

                        dbContext.SaveChanges();
                        if (releaseAmount > 0) {
                            await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist was released in {releaseAmount} channel(s)!"));
                            return;
                        }
                    }
                    await ic.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"No channels are subscribed to **{modlistMetadata.Title}**!"));
                }
            }
        }

    }
}
