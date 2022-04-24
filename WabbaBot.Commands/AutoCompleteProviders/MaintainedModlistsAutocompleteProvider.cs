using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WabbaBot.Core;

namespace WabbaBot.Commands {
    public class MaintainedModlistsAutocompleteProvider : IAutocompleteProvider {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
            using (var dbContext = new BotDbContext()) {
                var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == ctx.User.Id);
                if (maintainer == null) {
                    return Task.FromResult((IEnumerable<DiscordAutoCompleteChoice>)new List<DiscordAutoCompleteChoice>());
                }
                else {
                    dbContext.Entry(maintainer).Collection(m => m.ManagedModlists).Load();
                    var choices = Bot.Modlists.Where(m => maintainer.ManagedModlists.Any(lm => m.Links.MachineURL == lm.MachineURL))
                                                         .OrderBy(m => m.Title)
                                                         .Select(m => new DiscordAutoCompleteChoice(m.Title, m.Links.MachineURL))
                                                         .Take(25);
                    return Task.FromResult(choices);
                }
            }
        }
    }
}
