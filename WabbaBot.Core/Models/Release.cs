using WabbaBot.Core.Abstracts;
using WabbaBot.Models;

namespace WabbaBot.Core.Models {
    public class Release : ABaseModel {
        public ManagedModlist ManagedModlist { get; set; }
        public int ManagedModlistId { get; set; }
        public List<ReleaseMessage> ReleaseMessages { get; set; }
        public Release(int managedModlistId) {
            ManagedModlistId = managedModlistId;
        }
    }
}
