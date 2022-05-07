using Microsoft.EntityFrameworkCore;
using WabbaBot.Core.Abstracts;

namespace WabbaBot.Models {
    [Index(nameof(DiscordUserId), IsUnique = true)]
    public class Maintainer : ABaseModel {
        public ulong DiscordUserId { get; set; }
        public string? CachedName { get; set; } = null;
        public List<ManagedModlist> ManagedModlists { get; } = new List<ManagedModlist>();
        public List<ReleaseMessage> ReleaseMessages { get; } = new List<ReleaseMessage>();
        public Maintainer(ulong discordUserId, string cachedName) {
            DiscordUserId = discordUserId;
            CachedName = cachedName;
        }
    }
}
