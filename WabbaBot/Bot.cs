﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Wabbajack.DTOs;
using Wabbajack.DTOs.JsonConverters;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using WabbaBot.Models;
using System.Reflection;
using WabbaBot.Interfaces;
using WabbaBot.Commands;
using WabbaBot.Converters;

namespace WabbaBot {
    public class Bot {
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
        public static Dictionary<string, IModalResponse> ModalResponses { get; private set; }
        public static bool IsRunning { get; private set; }
        public static bool DebugModeEnabled { get; private set; }

        public Bot(BotSettings settings, bool debugModeEnabled) {
            Settings = settings;
            DebugModeEnabled = debugModeEnabled;

            _ = ReloadModlistsAsync();

            DiscordClient.Ready += EventHandlers.OnReady;
            DiscordClient.ClientErrored += EventHandlers.OnClientError;

            DiscordClient.UseInteractivity(new InteractivityConfiguration() {
                ResponseBehavior = DSharpPlus.Interactivity.Enums.InteractionResponseBehavior.Ack,
                Timeout = TimeSpan.FromSeconds(20),
                AckPaginationButtons = true
            });
            DiscordClient.GetInteractivity();

            ModalResponses = Assembly.GetExecutingAssembly()
                                     .GetExportedTypes()
                                     .Where(x => !x.IsInterface && !x.IsAbstract && typeof(IModalResponse).IsAssignableFrom(x))
                                     .Select(x => (IModalResponse)Activator.CreateInstance(x)!)
                                     .ToDictionary(x => x.ResponseId);
            DiscordClient.ModalSubmitted += DiscordClient_ModalSubmitted;

            Commands = DiscordClient.UseSlashCommands();
            Commands.RegisterCommands<SlashCommands>();

            Commands.SlashCommandExecuted += EventHandlers.OnCommandExecuted;
            Commands.SlashCommandErrored += EventHandlers.OnCommandErrored;
        }

        private async Task DiscordClient_ModalSubmitted(DiscordClient sender, DSharpPlus.EventArgs.ModalSubmitEventArgs e) {
            var id = e.Interaction?.Data?.CustomId ?? null;
            string? param = null;
            // Replace this sometime, dirty trick to get parameters from the initial interaction
            if (id?.Contains('|') ?? false) {
                var splitId = id.Split('|');
                id = splitId[0];
                param = splitId[1];
            }

            IModalResponse? modalResponse = null;
            if (id == null || !ModalResponses.TryGetValue(id, out modalResponse) || modalResponse == null) {
                DiscordClient.Logger.LogError($"Could not find modal response for id {id}!");
                return;
            }

            DiscordClient.Logger.LogInformation($"Scheduling modal response of type {modalResponse.GetType().Name} to respond to {id}");
            _ = Task.Run(async () => {
                await modalResponse!.Respond(sender, e, param) .ContinueWith((_) => DiscordClient.Logger.LogInformation($"Completed modal response of type {modalResponse.GetType().Name}"));
            });
        }

        public static async Task<bool> ReloadModlistsAsync(bool forceReload = false) {
            // Primarily for the AutocompleteProviders, don't go pulling the modlists jsons constantly
            if ((DateTime.UtcNow > LastModlistMetadataReload.AddSeconds(Settings.ModlistMetadataCacheTimeout)) || !Modlists.Any() || forceReload) {
                DiscordClient.Logger.LogInformation("Getting modlist repositories...");
                ModlistRepositories = await GetModlistRepositoriesAsync(new Uri(Consts.MODLIST_REPOSITORIES_URI));
                DiscordClient.Logger.LogInformation($"Retrieved {ModlistRepositories.Count} repositories.");

                DiscordClient.Logger.LogInformation($"Getting modlists...");
                Modlists = ModlistRepositories.AsParallel().SelectMany(repo => GetModlistMetadatasAsync(repo.Value).Result).ToList();
                DiscordClient.Logger.LogInformation($"Retrieved {Modlists.Count} modlists.");

                LastModlistMetadataReload = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public static async Task<Dictionary<string, Uri>> GetModlistRepositoriesAsync(Uri repositoriesURL) {
            var repositories = await _httpClient.GetFromJsonAsync<Dictionary<string, Uri>>(repositoriesURL);
            return repositories != null ? repositories : new Dictionary<string, Uri>();
        }

        public static async Task<ModlistMetadata[]> GetModlistMetadatasAsync(Uri modlistsMetadataURL, JsonSerializerOptions? options = null) {
            options ??= new JsonSerializerOptions() {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                Converters = {
                    // Wabbajack
                    new HashJsonConverter(),
                    new HashRelativePathConverter(),

                    // WabbaBot - 'compatibility' for unknown games
                    new GameJsonConverter()
                }
            };

            try {
                var modlistMetadatas = await _httpClient.GetFromJsonAsync<ModlistMetadata[]>(modlistsMetadataURL, options);
                return modlistMetadatas != null ? modlistMetadatas : Array.Empty<ModlistMetadata>();
            }
            catch (Exception ex) {
                DiscordClient.Logger.LogError($"Failed to load modlist metadata for repo {modlistsMetadataURL}: {ex.Message}");
                return Array.Empty<ModlistMetadata>();
            }
        }
        public async Task RunAsync() {
            if (!IsRunning) {
                IsRunning = true;
                DiscordClient.Logger.LogInformation("Starting bot...");
                await DiscordClient.ConnectAsync();
                DiscordClient.Logger.LogInformation("Connected to Discord!");

                // Background activity changing task
                _ = Task.Run(async () => {
                    while (true) {
                        DiscordClient.Logger.LogInformation("Updating status...");
                        var statusText = await UpdateStatusAsync();
                        DiscordClient.Logger.LogInformation($"Set status to '{statusText}'");
                        await Task.Delay(TimeSpan.FromSeconds(Settings.ActivityRefreshingTimeout));
                    }
                });
            }
        }

        public async Task StopAsync() {
            if (IsRunning) {
                IsRunning = false;
                await DiscordClient.DisconnectAsync();
            }
        }

        private async Task<string> UpdateStatusAsync() {
            using (var dbContext = new BotDbContext()) {
                /*
                string text = "Wabbajack modlists";
                var randomManagedModlist = dbContext.ManagedModlists.RandomOrDefault();
                if (randomManagedModlist != default(ManagedModlist)) {
                    await ReloadModlistsAsync();
                    var modlistMetadata = Modlists.FirstOrDefault(modlist => modlist.Links.MachineURL == randomManagedModlist.MachineURL);
                    if (modlistMetadata != default(ModlistMetadata))
                        text = modlistMetadata.Title;
                }
                */
                string text = "Oblivion Remastered";
                var activity = new DiscordActivity(text, ActivityType.Playing);
                await DiscordClient.UpdateStatusAsync(activity);
                return text;
            }
        }
    }

}
