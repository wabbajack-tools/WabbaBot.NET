using Newtonsoft.Json.Linq;
using WabbaBot.Models;

namespace WabbaBot {
    public static class JsonImporter {
        #nullable disable
        public static async Task<bool> ImportJsonDb(string serversJsonPath, string modlistsJsonPath) {
            JArray serversData = JArray.Parse(File.ReadAllText(serversJsonPath));
            JArray modlistsData = JArray.Parse(File.ReadAllText(modlistsJsonPath));
            var repos = await Bot.GetModlistRepositoriesAsync(new Uri(Consts.MODLIST_REPOSITORIES_URI));
            var modlists = repos.AsParallel().SelectMany(repo => Bot.GetModlistMetadatasAsync(repo.Value).Result).ToList();
            var existingMachineUrls = modlists.Select(m => m.Links.MachineURL).ToHashSet();
            using (var dbContext = new BotDbContext()) {
                // Import server data
                foreach(JObject server in serversData) {
                    foreach (JObject channel in server["listening_channels"]) {
                        var importedChannel = dbContext.SubscribedChannels.FirstOrDefault(sc => sc.DiscordChannelId == (ulong)channel["id"]);
                        if (importedChannel == default(SubscribedChannel)) {
                            importedChannel = new SubscribedChannel((ulong)channel["id"], (ulong)server["id"], "imported_channel");
                            dbContext.SubscribedChannels.Add(importedChannel);
                            dbContext.SaveChanges();
                        }
                        foreach(string machineURL in channel["listening_to"]) {
                            if (modlists.Select(m => m.Links.MachineURL).Contains(machineURL)) {
                                var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == machineURL);
                                if (managedModlist == default(ManagedModlist))
                                    managedModlist = new ManagedModlist(machineURL);

                                importedChannel.ManagedModlists.Add(managedModlist);
                                dbContext.SaveChanges();
                            }
                        }
                        if (!importedChannel.ManagedModlists.Any()) {
                            dbContext.SubscribedChannels.Remove(importedChannel);
                            dbContext.SaveChanges();
                        }

                        foreach (JProperty listRole in server["list_roles"]) {
                            var mm = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == listRole.Name);
                            if (mm != default(ManagedModlist))
                                dbContext.PingRoles.Add(new PingRole((ulong)listRole.Value, (ulong)server["id"], mm.Id));
                        }
                        dbContext.SaveChanges();
                    }
                }

                // Import maintainer data
                foreach(JObject modlist in modlistsData) {
                    var managedModlist = dbContext.ManagedModlists.FirstOrDefault(mm => mm.MachineURL == (string)modlist["id"]);
                    if (managedModlist != default(ManagedModlist)) {
                        var maintainer = dbContext.Maintainers.FirstOrDefault(m => m.DiscordUserId == (ulong)modlist["author_id"]);
                        if (maintainer == default(Maintainer)) {
                            maintainer = new Maintainer((ulong)modlist["author_id"], (string)modlist["author"]);
                            dbContext.Maintainers.Add(maintainer);
                            dbContext.SaveChanges();
                        }
                        managedModlist.Maintainers.Add(maintainer);
                    }
                }
                dbContext.SaveChanges();

            }
            return true;
        }
    }
}
