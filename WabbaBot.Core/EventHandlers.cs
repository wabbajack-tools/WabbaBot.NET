using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace WabbaBot.Core {
    public static class EventHandlers {
        public static Task OnReady(DiscordClient sender, ReadyEventArgs e) {
            sender.Logger.LogInformation("WabbaBot ready to process events!");

            return Task.CompletedTask;
        }
        public static Task OnClientError(DiscordClient sender, ClientErrorEventArgs e) {
            sender.Logger.LogError(e.Exception, "[ClientError]");

            return Task.CompletedTask;
        }
        public static Task OnCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e) {
            sender.Client.Logger.LogInformation($"[CommandExecuted] {e.Context.User.Username} ({e.Context.User.Id} in {e.Context.Guild.Name} ({e.Context.Guild.Id}): {e.Context.Interaction}'");

            return Task.CompletedTask;
        }
        public static async Task OnCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e) {
            e.Context.Client.Logger.LogError($"[CommandError] {e.Context.User.Username} tried executing '{e.Context.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");

            var errorPrefix = "**An error occurred!** ";
            if (e.Exception is SlashExecutionChecksFailedException slex) {
                foreach (var check in slex.FailedChecks) {
                    /*
                    if (check is Attribute) {
                        await e.Context.CreateResponseAsync($"{errorPrefix} You do not have the permissions required to execute this command.");
                    }
                    */
                    await e.Context.CreateResponseAsync($"{errorPrefix} You do not have the permissions required to execute this command.");
                }
            }
            /*
            else if (e.Exception is ArgumentException) {
                StringBuilder message = new StringBuilder();
                message.AppendLine($"Too few arguments for command `{e.Command.Name}`!");
                foreach (var overload in e.Command.Overloads) {
                    message.Append($"Usage: `/{e.Command.Name}");
                    foreach (var argument in overload.Arguments) {
                        message.Append($" <{argument.Name}>");
                    }
                    message.Append("`");
                }
                await e.Context.RespondAsync(message.ToString());
            }
            else
                await e.Context.RespondAsync($"{errorPrefix} {e.Exception.Message}");
            */

        }

    }
}

