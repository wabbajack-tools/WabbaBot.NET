#nullable disable
namespace WabbaBot.Models {
    public class Release : ABaseModel {
        public ManagedModlist ManagedModlist { get; set; }
        public int ManagedModlistId { get; set; }
        public List<ReleaseMessage> ReleaseMessages { get; set; } = new List<ReleaseMessage>();
        public Release(int managedModlistId) {
            ManagedModlistId = managedModlistId;
        }
    }
}
