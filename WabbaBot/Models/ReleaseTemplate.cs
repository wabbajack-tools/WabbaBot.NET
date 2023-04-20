#nullable disable

namespace WabbaBot.Models {
    public class ReleaseTemplate : ABaseModel {
        public ManagedModlist ManagedModlist { get; set; }
        public int ManagedModlistId { get; set; }
        public string Content { get; set; }
    }
}
