using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WabbaBot.Models {
    [Index(nameof(DiscordRoleId), IsUnique = true)]
    public class PingRole {
        [Key]
        public int Id { get; set; }
        public ManagedModlist ManagedModlist { get; set; }
        public ulong DiscordRoleId { get; set; }
        public ulong DiscordGuildId { get; set; }
        public PingRole(ulong discordRoleId, ulong discordGuildId) {
            DiscordRoleId = discordRoleId;
            DiscordGuildId = discordGuildId;
        }
    }
}
