using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WabbaBot.Core.Interfaces;

namespace WabbaBot.Models {
    [Index(nameof(DiscordUserId), IsUnique = true)]
    public partial class Maintainer : IHasId {
        [Key]
        public int Id { get; set; }
        public ulong DiscordUserId { get; set; }
        public string? CachedName { get; set; } = null;
        public List<ManagedModlist> ManagedModlists { get; } = new List<ManagedModlist>();
        public Maintainer(ulong discordUserId, string cachedName) {
            DiscordUserId = discordUserId;
            CachedName = cachedName;
        }
    }
}
