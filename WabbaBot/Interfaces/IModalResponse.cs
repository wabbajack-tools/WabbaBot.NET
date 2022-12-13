using DSharpPlus;
using DSharpPlus.EventArgs;

namespace WabbaBot.Interfaces {
    public interface IModalResponse {
        public string ResponseId { get; }
        public Task Respond(DiscordClient client, ModalSubmitEventArgs e, string? param = null);
    }
}
