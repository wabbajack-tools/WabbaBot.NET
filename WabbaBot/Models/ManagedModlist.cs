using Microsoft.EntityFrameworkCore;

namespace WabbaBot.Models {
    [Index(nameof(MachineURL), IsUnique = true)]
    public class ManagedModlist : ABaseModel {
        public string MachineURL { get; set; }
        public List<Maintainer> Maintainers { get; } = new List<Maintainer>();
        public List<SubscribedChannel> SubscribedChannels { get; } = new List<SubscribedChannel>();
        public List<PingRole> PingRoles { get; } = new List<PingRole>();
        public List<ReleaseMessage> ReleaseMessages { get; } = new List<ReleaseMessage>();
        public ReleaseTemplate ReleaseTemplate { get; set; }

        public ManagedModlist(string machineURL) {
            MachineURL = machineURL;
        }
    }
}
