using DSharpPlus.SlashCommands;
using Wabbajack.DTOs;

namespace WabbaBot.Exceptions {
    public class MaintainersOnlyCommandException : ACommandException {
        public MaintainersOnlyCommandException(InteractionContext ic) : base(ic, $"You must be maintaining this modlist to use that command!") {
        }
    }
}
