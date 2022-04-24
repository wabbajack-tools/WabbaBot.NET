using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WabbaBot.Core.Interfaces;

namespace WabbaBot.Models {
    [Index(nameof(DiscordChannelId), IsUnique = true)]
    public class SubscribedChannel : IHasId {
        [Key]
        public int Id { get; set; }
        public ulong DiscordChannelId { get; set; }
        public ulong DiscordGuildId { get; set; }
        public string? CachedName { get; set; } = null;
        public List<ManagedModlist> ManagedModlists { get; } = new List<ManagedModlist>();
        public SubscribedChannel(ulong discordChannelId, ulong discordGuildId, string cachedName) {
            DiscordChannelId = discordChannelId;
            DiscordGuildId = discordGuildId;
            CachedName = cachedName;
        }

    }
}
