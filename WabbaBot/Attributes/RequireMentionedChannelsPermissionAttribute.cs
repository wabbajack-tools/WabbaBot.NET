using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using WabbaBot;
using WabbaBot.Models;

namespace WabbaBot.Commands.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class RequireMentionedChannelsPermissionAttribute : SlashCheckBaseAttribute {
        public DSharpPlus.Permissions Permissions;
        public RequireMentionedChannelsPermissionAttribute(DSharpPlus.Permissions permissions) {
            Permissions = permissions;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ic) {
            //var botPerms = ic.ResolvedChannelMentions.First().PermissionsFor(ic.Guild.CurrentMember);
            var botPerms = ic.Guild.CurrentMember.PermissionsIn(ic.ResolvedChannelMentions.First());
            return botPerms.HasPermission(Permissions);
            //ic.ResolvedChannelMentions.All(dc => dc.PermissionsFor(ic.Guild.CurrentMember).HasFlag(Permissions));
        }
    }
}
