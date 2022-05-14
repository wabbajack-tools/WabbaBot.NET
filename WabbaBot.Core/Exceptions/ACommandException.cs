using DSharpPlus.SlashCommands;
using System.Text;

namespace WabbaBot.Core.Exceptions {
    public abstract class ACommandException : Exception {
        private string _message;
        private string _discordMessage;
        public InteractionContext InteractionContext { get; private set; }
        public virtual string DiscordMessage {
            get => _discordMessage == null ? Message : _discordMessage;
            protected set {
                _discordMessage = value;
                var sb = new StringBuilder(_discordMessage);
                sb.Replace("*", string.Empty);
                sb.Replace("__", string.Empty);
                sb.Replace("~~", string.Empty);
                sb.Replace("`", string.Empty);
                sb.Replace(">", string.Empty);
                _message = sb.ToString();
            }
        }
        public override string Message => _message;
        public ACommandException(InteractionContext interactionContext, string discordMessage) : base() {
            InteractionContext = interactionContext;
            DiscordMessage = discordMessage;
        }
    }
}
