using Microsoft.EntityFrameworkCore;

namespace WabbaBot.Models {
    [Index(nameof(DiscordChannelId), IsUnique = true)]
    public class SubscribedChannel : ABaseModel {
        public ulong DiscordChannelId { get; set; }
        public ulong DiscordGuildId { get; set; }
        public string? CachedName { get; set; } = null;
        public List<ManagedModlist> ManagedModlists { get; } = new List<ManagedModlist>();
        public List<ReleaseMessage> ReleaseMessages { get; } = new List<ReleaseMessage>();
        public SubscribedChannel(ulong discordChannelId, ulong discordGuildId, string cachedName) {
            DiscordChannelId = discordChannelId;
            DiscordGuildId = discordGuildId;
            CachedName = cachedName;
        }

    }
}
