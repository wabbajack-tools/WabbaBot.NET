using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using System.Text;
using WabbaBot.Attributes;

namespace WabbaBot {
    public static class EventHandlers {
        public static async Task OnReady(DiscordClient sender, ReadyEventArgs e) {
            sender.Logger.LogInformation("[Ready] WabbaBot ready to process events!");
        }
        public static async Task OnClientError(DiscordClient sender, ClientErrorEventArgs e) {
            sender.Logger.LogError(e.Exception, "[ClientError]");
        }
        public static async Task OnCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e) {
            sender.Client.Logger.LogInformation($"[CommandExecuted] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {(e.Context.Interaction.Data.Options != null ? string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value)) : string.Empty)}'");
        }
        public static async Task OnCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e) {
            var messageBuilder = new StringBuilder(Consts.ERROR_MESSAGE_PREFIX + " ");
            if (e.Exception is SlashExecutionChecksFailedException slex) {
                var check = slex.FailedChecks.First();
                switch (check) {
                    case RequireBotAdministratorAttribute:
                        messageBuilder.Append("You must be a bot administrator to use that command!");
                        break;
                    case RequireModlistMaintainerAttribute:
                        messageBuilder.Append("You must be maintaining this modlist to use that command!");
                        break;
                    case RequireMentionedChannelMessagingPermissionsAttribute:
                        messageBuilder.Append("I either don't have permission to view that channel or I can't send messages there!");
                        break;
                    case SlashRequireUserPermissionsAttribute:
                        messageBuilder.Append("You don't have permission to use that command!");
                        break;
                    default:
                        messageBuilder.Append(e.Exception.Message);
                        break;
                }
                var message = messageBuilder.ToString();
                sender.Client.Logger.LogError($"[CommandErrored] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value))}' execution check failed:\n{message}");
                await e.Context.CreateResponseAsync(message);
            }
            else if (e.Exception is BadRequestException bre) {
                sender.Client.Logger.LogError($"[CommandErrored] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value))}' errored:\n{e.Exception.GetType()}\n{e.Exception.Message}\n{e.Exception.StackTrace} with JsonMessage\n{bre.JsonMessage}\n and other errors:\n{bre.Errors}");
            }
            else if (e.Exception is DiscordException de) {
                sender.Client.Logger.LogError($"[CommandErrored] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value))}' errored:\n{e.Exception.GetType()}\n{e.Exception.Message}\n{e.Exception.StackTrace} with JsonMessage {de.JsonMessage}");
            }
            else {
                sender.Client.Logger.LogError($"[CommandErrored] {e.Context.User.Username} ({e.Context.User.Id}) in {e.Context.Guild.Name}: '/{e.Context.CommandName} {string.Join(' ', e.Context.Interaction.Data.Options.Select(option => option.Value))}' errored:\n{e.Exception.GetType()}\n{e.Exception.Message}\n{e.Exception.StackTrace}");
            }
        }
    }
}

