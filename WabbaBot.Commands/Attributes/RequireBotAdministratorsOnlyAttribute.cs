using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot.Core;
using WabbaBot.Models;

namespace WabbaBot.Commands.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class RequireBotAdministratorsOnlyAttribute : SlashCheckBaseAttribute {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) => Task.FromResult(Bot.Settings.Administrators?.Contains(ctx.User.Id) ?? false);
    }
}
