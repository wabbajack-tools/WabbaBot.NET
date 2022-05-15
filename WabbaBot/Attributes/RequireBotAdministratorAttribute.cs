using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot;
using WabbaBot.Models;

namespace WabbaBot.Commands.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class RequireBotAdministratorAttribute : SlashCheckBaseAttribute {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ic) => Bot.Settings.Administrators?.Contains(ic.User.Id) ?? false;
    }
}
