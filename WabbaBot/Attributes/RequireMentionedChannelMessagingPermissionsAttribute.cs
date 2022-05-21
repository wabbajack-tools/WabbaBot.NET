using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WabbaBot.Attributes {
    class RequireMentionedChannelMessagingPermissionsAttribute : SlashCheckBaseAttribute {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ic) {
            var option = ic.Interaction.Data.Options?.FirstOrDefault(option => option.Type == DSharpPlus.ApplicationCommandOptionType.Channel) ?? default(DiscordInteractionDataOption);
            if (option == default(DiscordInteractionDataOption))
                return false;

            try {
                var channel = await ic.Client.GetChannelAsync((ulong)option.Value);
                var message = await channel.SendMessageAsync("I'm doing a permission check to see if I can send messages in this channel - don't mind me, this message should be removed soon!");
                await message.DeleteAsync();
            }
            catch {
                return false;
            }
            return true;
        }
    }
}
