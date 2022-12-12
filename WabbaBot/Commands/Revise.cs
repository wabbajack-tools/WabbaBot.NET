using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WabbaBot.Attributes;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [RequireModlistMaintainer]
        [SlashCommand(nameof(Revise), "Revise one of the release messages for the specified list")]
        public async Task Revise(InteractionContext ic, [Option("Modlist", "The modlist you want to revise a release message of", true), Autocomplete(typeof(MaintainedModlistsAutocompleteProvider))] string machineURL, [Option("Message", "The release message you want to send out. Markdown supported!")] string message) {
            using (var dbContext = new BotDbContext()) {
                var latestRelease = dbContext.Releases.Include(rmg => rmg.ManagedModlist).OrderByDescending(rmg => rmg.CreatedOn).FirstOrDefault(rmg => rmg.ManagedModlist.MachineURL == machineURL);
                if (latestRelease == default(Release)) {
                    var managedModlist = dbContext.ManagedModlists.FirstOrDefault(managedModlist => managedModlist.MachineURL == machineURL);
                    if (managedModlist == default(ManagedModlist)) {
                        await ic.CreateResponseAsync("This modlist either doesn't exist or it isn't maintained by anybody!");
                    }
                    await ic.CreateResponseAsync($"No releases found for modlist **{machineURL}**!");
                }
                dbContext.Entry(latestRelease!).Collection(r => r.ReleaseMessages).Load();

                int amountEdited = 0;
                foreach(var releaseMessage in latestRelease!.ReleaseMessages) {
                    dbContext.Entry(releaseMessage).Reference(rm => rm.SubscribedChannel).Load();
                    try {
                        var discordGuild = await ic.Client.GetGuildAsync(releaseMessage.SubscribedChannel.DiscordGuildId);
                        var discordChannel = discordGuild.GetChannel(releaseMessage.SubscribedChannel.DiscordChannelId);
                        var discordMessage = await discordChannel.GetMessageAsync(releaseMessage.DiscordMessageId);
                        var newEmbed = new DiscordEmbedBuilder(discordMessage.Embeds.First());
                        newEmbed.Description = message.Replace(@"\n", "\n");
                        var editedMessage = await discordMessage.ModifyAsync(newEmbed.Build());
                        if (editedMessage != default) {
                            releaseMessage.Message = message;
                            amountEdited++;
                        }
                    }
                    catch (Exception ex) {
                        ic.Client.Logger.LogError($"Could not revise a message for {machineURL} with message ID {releaseMessage.DiscordMessageId} Exception: {ex.Message}\n{ex.StackTrace}");
                    }
                }
                if (amountEdited > 0) {
                    dbContext.SaveChanges();
                    await ic.CreateResponseAsync($"Succesfully revised {latestRelease.ReleaseMessages.Count} message(s).");
                }
                else {
                    await ic.CreateResponseAsync($"Failed to revise any messages for the last release - the release messages may have been deleted or I no longer have access to any channels/servers with release messages.");
                }
            }
        }

    }
}
