using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using WabbaBot.Helpers;
using WabbaBot.Interfaces;
using WabbaBot.Models;

namespace WabbaBot.ModalResponses {
    public class ReleaseModalResponse : IModalResponse {
        public string ResponseId => nameof(Commands.SlashCommands.Release);
        public async Task Respond(DiscordClient client, ModalSubmitEventArgs e, string? machineURL) {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var message = e.Values["message"];
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

                    DiscordEmbed embed = await DiscordHelper.GetReleaseEmbedForModlist(e.Interaction, message, modlistMetadata);

                    dbContext.Entry(managedModlist).Collection(lm => lm.SubscribedChannels).Load();
                    if (managedModlist.SubscribedChannels.Any()) {

                        var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == e.Interaction.User.Id);
                        if (maintainer == default(Maintainer)) {
                            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Maintainer not found. This error should never appear in the first place, what's going on?!"));
                            return;
                        }


                        var release = new Release(managedModlist.Id);
                        var dbGroup = dbContext.Releases.Add(release);
                        dbContext.SaveChanges();

                        int releaseAmount = 0;
                        foreach (var subscribedChannel in managedModlist.SubscribedChannels) {
                            try {
                                var discordChannel = await client.GetChannelAsync(subscribedChannel.DiscordChannelId);
                                var discordMessage = await discordChannel.SendMessageAsync(embed);
                                if (discordMessage != default) {
                                    var releaseMessage = new ReleaseMessage(message, discordMessage.Id, managedModlist.Id, subscribedChannel.Id, maintainer.Id, dbGroup.Entity.Id);
                                    var pingRole = dbContext.PingRoles.FirstOrDefault(pr => pr.ManagedModlistId == managedModlist.Id && pr.DiscordGuildId == discordChannel.Guild.Id);
                                    if (pingRole != default(PingRole)) {
                                        try {
                                            var role = e.Interaction.Guild.GetRole(pingRole.DiscordRoleId);
                                            await discordChannel.SendMessageAsync(role.Mention);
                                        }
                                        catch (Exception ex) {
                                            await discordChannel.SendMessageAsync("I wanted to ping a role here, but it seems like it no longer exists! Set a new one with /setrole. Alternatively, use /clearrole to confirm you don't want any roles to be pinged when this modlist releases, and to prevent this message from showing up again.");
                                            client.Logger.LogError($"Role with database id {pingRole.Id} could not be pinged! Exception: {ex.Message}\n{ex.StackTrace}");
                                        }
                                    }
                                    dbContext.ReleaseMessages.Add(releaseMessage);
                                    releaseAmount++;
                                }
                            }
                            catch (Exception ex) {
                                client.Logger.LogError($"Could not release a message for {modlistMetadata.Title} in {subscribedChannel.CachedName} ({subscribedChannel.DiscordChannelId}) with guild id {subscribedChannel.DiscordGuildId}. Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                        if (releaseAmount == 0)
                            dbContext.Releases.Remove(release);

                        dbContext.SaveChanges();
                        if (releaseAmount > 0) {
                            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Modlist was released in {releaseAmount} channel(s)!"));
                            return;
                        }
                    }
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"No channels are subscribed to **{modlistMetadata.Title}**!"));
                }
            }
        }
    }
}
