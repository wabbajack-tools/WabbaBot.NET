using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace WabbaBot.Helpers {
    public static class ImageProcessing {
        private static readonly HttpClient _httpClient = new();

        public static async Task<Color> GetColorFromImageUrlAsync(string imageUrl) {
            Image<Rgb24>? image = null;
            try {
                Stream bytes = await _httpClient.GetStreamAsync(imageUrl);
                image = await Image.LoadAsync<Rgb24>(bytes);
                if (image == null)
                    return Color.Black; // If we can't load the image, return black.

                image.Mutate(
                    img => img
                        .Resize(new ResizeOptions {
                            Size = new Size(32, 0), // Reduce image size for quantization speed.
                        })
                        .Quantize(new OctreeQuantizer(new QuantizerOptions {
                            Dither = null, // We don't want any dithering.
                            DitherScale = 0,
                            MaxColors = 1 // We only want the most common colour.
                    })));

                return image[0, 0]; // Get the colour from the first pixel.
            }
            catch {
                return Color.Black;
            }
        }
    }
}