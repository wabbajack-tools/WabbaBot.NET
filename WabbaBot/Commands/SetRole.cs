using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WabbaBot.AutocompleteProviders;
using WabbaBot.Models;

namespace WabbaBot {
    public partial class Commands : ApplicationCommandModule {
        [SlashRequireUserPermissions(Permissions.ManageRoles)]
        [SlashCommand(nameof(SetRole), "Set a role to mention/ping whenever the specified modlist is released")]
        public async Task SetRole(InteractionContext ic, [Option("Modlist", "The modlist to receive mentions/pings for on release notifications", true), Autocomplete(typeof(ManagedModlistsAutocompleteProvider))] string machineURL, [Option("Role", "The role that should be mentioned/pinged when the specified modlist releases")] DiscordRole discordRole) {
            using (var dbContext = new BotDbContext()) {
                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                if (managedModlist == default(ManagedModlist)) {
                    await ic.CreateResponseAsync($"Modlist {machineURL} is not managed by WabbaBot!");
                    return;
                }
                dbContext.Entry(managedModlist).Collection(lm => lm.SubscribedChannels).Load();
                if (!managedModlist.SubscribedChannels.Any(sc => sc.DiscordGuildId == ic.Guild.Id)) {
                    await ic.CreateResponseAsync($"No channels are subscribed to {machineURL}! Please subscribe to the modlist prior to setting a mention/ping role.");
                    return;
                }

                var role = dbContext.PingRoles.FirstOrDefault(pr => pr.DiscordRoleId == discordRole.Id);
                if (role == default(PingRole)) {
                    role = new PingRole(discordRole.Id, ic.Guild.Id, managedModlist.Id);
                    dbContext.PingRoles.Add(role);
                }
                else {
                    role.ManagedModlistId = managedModlist.Id;
                    dbContext.Entry(role).State = EntityState.Modified;
                }

                dbContext.SaveChanges();
                await ic.CreateResponseAsync($"Release notifications for {machineURL} will now ping the **{discordRole.Name}** role.");
            }
        }

    }
}
