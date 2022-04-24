using System.ComponentModel.DataAnnotations;
using Wabbajack.DTOs;
using WabbaBot.Core;
using Microsoft.EntityFrameworkCore;
using WabbaBot.Core.Interfaces;

namespace WabbaBot.Models {
    [Index(nameof(MachineURL), IsUnique = true)]
    public class ManagedModlist : IHasId {
        [Key]
        public int Id { get; set; }
        public string MachineURL { get; set; }
        public List<Maintainer> Maintainers { get; } = new List<Maintainer>();
        public List<SubscribedChannel> SubscribedChannels { get; } = new List<SubscribedChannel>();
        public List<PingRole> PingRoles { get; set; } = new List<PingRole>();
        public ManagedModlist(string machineURL) {
            MachineURL = machineURL;
        }
    }
}
