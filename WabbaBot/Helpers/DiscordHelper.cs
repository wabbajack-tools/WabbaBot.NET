using DSharpPlus.Entities;
using Wabbajack.DTOs;

namespace WabbaBot.Helpers {
    internal class DiscordHelper {
        public static async Task<DiscordEmbed> GetReleaseEmbedForModlist(DiscordInteraction ic, string message, ModlistMetadata modlistMetadata) {
            var color = await ImageProcessing.GetColorFromImageUrlAsync(modlistMetadata.Links.ImageUri);
            DiscordEmbed embed = new DiscordEmbedBuilder() {
                Title = $"{ic.User.Username} just released {modlistMetadata.Title} v{modlistMetadata.Version}!",
                Timestamp = DateTime.Now,
                Description = message,
                ImageUrl = modlistMetadata.Links.ImageUri,
                Color = new DiscordColor(color.ToHex().Remove(6, 2)),
                Footer = new DiscordEmbedBuilder.EmbedFooter() {
                    Text = "WabbaBot"
                }
            }.Build();

            return embed;
        }

    }
}
