using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [SlashRequireUserPermissions(Permissions.ManageRoles)]
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
    }
}
