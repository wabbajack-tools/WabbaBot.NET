using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Core;
using WabbaBot.Core.Exceptions;
using WabbaBot.Models;

namespace WabbaBot.Commands.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class BotAdministratorsOnlyAttribute : SlashCheckBaseAttribute {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ic) {
            if (!Bot.Settings.Administrators?.Contains(ic.User.Id) ?? true)
                throw new BotAdministratorsOnlyCommandException(ic);

            return true;
        }
    }
}
