using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using System.Text;
using WabbaBot.Core.Exceptions;

namespace WabbaBot.Core {
    public static class EventHandlers {
        public static async Task OnReady(DiscordClient sender, ReadyEventArgs e) {
            sender.Logger.LogInformation("WabbaBot ready to process events!");
        }
        public static async Task OnClientError(DiscordClient sender, ClientErrorEventArgs e) {
            sender.Logger.LogError(e.Exception, "[ClientError]");
        }
        public static async Task OnCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e) {
            sender.Client.Logger.LogInformation($"[CommandExecuted] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value))}'");
        }
        public static async Task OnCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e) {
            sender.Client.Logger.LogError($"[CommandErrored] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value))}' errored:\n{e.Exception.GetType()}\n{e.Exception.Message}\n{e.Exception.StackTrace}");
            const string errorPrefix = "**An error occurred!**";
            if (e.Exception is ACommandException ex)
                await e.Context.CreateResponseAsync($"{errorPrefix} {ex.DiscordMessage}");
        }
    }
}

