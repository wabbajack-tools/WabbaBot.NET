using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace WabbaBot.Models {
    [Index(nameof(ManagedModlistId), IsUnique = true)]
    public class ModlistColor : ABaseModel {
        public ManagedModlist ManagedModlist { get; set; }
        public int ManagedModlistId { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public Color Color => Color.FromRgb(R, G, B);
        public string? ImageSourceUrl { get; set; } = null;
        
        public ModlistColor(int managedModlistId, byte r, byte g, byte b, string? imageSourceUrl = null) {
            ManagedModlistId = managedModlistId;
            R = r;
            G = g;
            B = b;
            ImageSourceUrl = imageSourceUrl;
        }
        public static async Task<ModlistColor> FromManagedModlist(int managedModlistId, string uri) {
            var httpClient = new HttpClient();
            Image<Rgb24>? image = null;
            try {
                Stream bytes = await httpClient.GetStreamAsync(uri);
                image = await Image.LoadAsync<Rgb24>(bytes);
                if (image == null)
                    return new ModlistColor(managedModlistId, 0, 0, 0, uri.ToString()); // If we can't load the image, return black.

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

                var pixel = image[0, 0]; // Get the colour from the first pixel.
                return new ModlistColor(managedModlistId, pixel.R, pixel.G, pixel.B, uri);
            }
            catch {
                return new ModlistColor(managedModlistId, 0, 0, 0);
                //return Color.Black;
            }
        }
    }
}
