using Microsoft.EntityFrameworkCore;
using WabbaBot.Core.Abstracts;

namespace WabbaBot.Models {
    [Index(nameof(DiscordMessageId), IsUnique = true)]
    public class ReleaseMessage : ABaseModel {
        public string Message { get; set; }
        public ulong DiscordMessageId { get; set; }
        public ManagedModlist ManagedModlist { get; set; }
        public int ManagedModlistId { get; set; }
        public SubscribedChannel SubscribedChannel { get; set; }
        public int SubscribedChannelId { get; set; }
        public Maintainer Maintainer { get; set; }
        public int MaintainerId { get; set; }
        public ReleaseMessage(string message, ulong discordMessageId, int managedModlistId, int subscribedChannelId, int maintainerId) {
            Message = message;
            DiscordMessageId = discordMessageId;
            ManagedModlistId = managedModlistId;
            SubscribedChannelId = subscribedChannelId;
            MaintainerId = maintainerId;
        }
    }
}
