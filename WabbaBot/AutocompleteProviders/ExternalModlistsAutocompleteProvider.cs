using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot;

namespace WabbaBot.AutocompleteProviders {
    public class ExternalModlistsAutocompleteProvider : IAutocompleteProvider {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
            await Bot.ReloadModlistsAsync();
            var choices = Bot.Modlists.Where(m => !string.IsNullOrEmpty(m.Title) && m.Title.StartsWith((string)ctx.OptionValue, StringComparison.OrdinalIgnoreCase))
                                                 .OrderBy(m => m.Title).Select(m => new DiscordAutoCompleteChoice(m.Title, m.Links.MachineURL))
                                                 .Take(Consts.DISCORD_MAX_AUTOCOMPLETE_OPTIONS);
            return choices;
        }
    }
}
