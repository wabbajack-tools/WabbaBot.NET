using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot.Core;

namespace WabbaBot.Commands.AutocompleteProviders {
    public class ExternalModlistsAutocompleteProvider : IAutocompleteProvider {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
            await Bot.ReloadModlists();
            var choices = Bot.Modlists.Where(m => !string.IsNullOrEmpty(m.Title) && m.Title.StartsWith(ctx.OptionValue.ToString(), StringComparison.OrdinalIgnoreCase))
                                                 .OrderBy(m => m.Title).Select(m => new DiscordAutoCompleteChoice(m.Title, m.Links.MachineURL))
                                                 .Take(Consts.DISCORD_MAX_AUTOCOMPLETE_OPTIONS);
            return choices;
        }
    }
}
