using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot.Core;

namespace WabbaBot.Commands {
    public class ManagedModlistsAutocompleteProvider : IAutocompleteProvider {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
            using (var context = new BotDbContext()) {
                var managedModlists = context.ManagedModlists.ToList();
                var choices = Bot.Modlists.Where(m => m.Title.StartsWith(ctx.OptionValue.ToString(), StringComparison.OrdinalIgnoreCase) && managedModlists.Any(lm => m.Links.MachineURL == lm.MachineURL))
                                                     .OrderBy(m => m.Title)
                                                     .Select(modlist => new DiscordAutoCompleteChoice(modlist.Title, modlist.Links.MachineURL))
                                                     .Take(25);

                return Task.FromResult(choices);
            }
        }
    }
}
