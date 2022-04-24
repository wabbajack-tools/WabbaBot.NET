using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Wabbajack.DTOs;
using Wabbajack.DTOs.JsonConverters;
using WabbaBot.Commands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;

namespace WabbaBot.Core {
    public class Bot {
        #region Properties
        private static HttpClient _httpClient = new HttpClient();

        private static DiscordClient? _discordClient = null;
        public static DiscordClient DiscordClient {
            get {
                _discordClient ??= new DiscordClient(new DiscordConfiguration() { Token = Settings.Token, TokenType = TokenType.Bot, Intents = DiscordIntents.AllUnprivileged });
                return _discordClient;
            }
        }
        public CommandsNextConfiguration CommandsConfiguration { get; private set; }
        public SlashCommandsExtension Commands { get; private set; }
        public static BotSettings Settings { get; private set; }
        public static DateTime LastModlistMetadataReload { get; private set; }
        public static Dictionary<string, Uri> ModlistRepositories { get; private set; }
        public static List<ModlistMetadata> Modlists { get; private set; }
        //public static BotDbContext DbContext { get; } = new BotDbContext();
        public static bool IsRunning { get; private set; }
        #endregion

        public Bot(BotSettings settings) {
            Settings = settings;
            Task.Run(async () => {
                await ReloadModlists();
            });

            DiscordClient.Ready += EventHandlers.OnReady;
            DiscordClient.ClientErrored += EventHandlers.OnClientError;

            DiscordClient.UseInteractivity(new InteractivityConfiguration() {
                AckPaginationButtons = true,
                ResponseBehavior = DSharpPlus.Interactivity.Enums.InteractionResponseBehavior.Ack,
                Timeout = TimeSpan.FromSeconds(20),
            });

            Commands = DiscordClient.UseSlashCommands();
            Commands.RegisterCommands<SlashCommands>(967359750180327494);
            Commands.RegisterCommands<SlashCommands>(810618941947904000);

            Commands.SlashCommandExecuted += EventHandlers.OnCommandExecuted;
            Commands.SlashCommandErrored += EventHandlers.OnCommandErrored;
        }

        public static async Task<bool> ReloadModlists(bool forceReload = false) {
            // Primarily for the AutocompleteProviders, don't go pulling the modlists jsons constantly
            if ((DateTime.Now > LastModlistMetadataReload.AddSeconds(Settings.ModlistMetadataCacheTimeout)) || forceReload) {
                DiscordClient.Logger.LogInformation("Getting modlist repositories...");
                ModlistRepositories = await GetModlistRepositories(new Uri(Settings.RepositoriesURL));
                DiscordClient.Logger.LogInformation($"Retrieved {ModlistRepositories.Count} repositories.");

                DiscordClient.Logger.LogInformation($"Getting modlists...");
                Modlists = ModlistRepositories.AsParallel().SelectMany(repo => GetModlistMetadatas(repo.Value).Result).ToList();
                DiscordClient.Logger.LogInformation($"Retrieved {Modlists.Count} modlists.");

                LastModlistMetadataReload = DateTime.Now;
                return true;
            }
            return false;
        }

        #region Methods

        private static async Task<Dictionary<string, Uri>> GetModlistRepositories(Uri repositoriesURL) {
            var repositories = await _httpClient.GetFromJsonAsync<Dictionary<string, Uri>>(repositoriesURL);
            return repositories != null ? repositories : new Dictionary<string, Uri>();
        }

        private static async Task<ModlistMetadata[]> GetModlistMetadatas(Uri modlistsMetadataURL, JsonSerializerOptions? options = null) {
            options ??= new JsonSerializerOptions() {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                Converters = {
                    new HashJsonConverter(),
                    new HashRelativePathConverter()
                }
            };

            var modlistMetadatas = await _httpClient.GetFromJsonAsync<ModlistMetadata[]>(modlistsMetadataURL, options);
            return modlistMetadatas != null ? modlistMetadatas : new ModlistMetadata[] { };
        }

        public async Task Run() {
            if (!IsRunning) {
                IsRunning = true;
                var activity = new DiscordActivity("Wabbajack modlists", ActivityType.Playing);
                await DiscordClient.ConnectAsync(activity);
            }
        }

        public async Task Stop() {
            if (IsRunning) {
                IsRunning = false;
                await DiscordClient.DisconnectAsync();
            }
        }
        #endregion

    }

}
