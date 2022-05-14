using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot;

namespace WabbaBot.Commands.AutocompleteProviders {
    public class MaintainedModlistsAutocompleteProvider : IAutocompleteProvider {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
            using (var dbContext = new BotDbContext()) {
                var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == ctx.User.Id);
                if (maintainer == null) {
                    return new List<DiscordAutoCompleteChoice>();
                }
                else {
                    dbContext.Entry(maintainer).Collection(m => m.ManagedModlists).Load();
                    var choices = Bot.Modlists.Where(m => maintainer.ManagedModlists.Any(lm => m.Links.MachineURL == lm.MachineURL))
                                                         .OrderBy(m => m.Title)
                                                         .Select(m => new DiscordAutoCompleteChoice(m.Title, m.Links.MachineURL))
                                                         .Take(Consts.DISCORD_MAX_AUTOCOMPLETE_OPTIONS);
                    return choices;
                }
            }
        }
    }
}
