using Microsoft.EntityFrameworkCore;

namespace WabbaBot.Models {
    [Index(nameof(DiscordRoleId), IsUnique = true)]
    public class PingRole : ABaseModel {
        public ulong DiscordRoleId { get; set; }
        public ulong DiscordGuildId { get; set; }
        public int ManagedModlistId { get; set; }
        public ManagedModlist? ManagedModlist { get; set; }
        public PingRole(ulong discordRoleId, ulong discordGuildId, int managedModlistId) {
            DiscordRoleId = discordRoleId;
            DiscordGuildId = discordGuildId;
            ManagedModlistId = managedModlistId;
        }
    }
}
