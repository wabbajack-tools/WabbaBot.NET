using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot;

namespace WabbaBot.Commands.AutocompleteProviders {
    public class ManagedModlistsAutocompleteProvider : IAutocompleteProvider {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
            using (var context = new BotDbContext()) {
                var managedModlists = context.ManagedModlists.ToList();
                if (!managedModlists.Any())
                    return new List<DiscordAutoCompleteChoice>();

                var choices = Bot.Modlists.Where(m => !string.IsNullOrEmpty(m.Title) && m.Title.StartsWith((string)ctx.OptionValue, StringComparison.OrdinalIgnoreCase) && managedModlists.Any(lm => m.Links.MachineURL == lm.MachineURL))
                                                     .OrderBy(m => m.Title)
                                                     .Select(modlist => new DiscordAutoCompleteChoice(modlist.Title, modlist.Links.MachineURL))
                                                     .Take(Consts.DISCORD_MAX_AUTOCOMPLETE_OPTIONS);

                return choices;
            }
        }
    }
}
