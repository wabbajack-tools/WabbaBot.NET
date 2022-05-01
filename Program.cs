using Newtonsoft.Json;
using WabbaBot.Core;

namespace WabbaBot
{
    class Program
    {
        static void Main() => MainAsync().GetAwaiter().GetResult();
        internal static async Task MainAsync() {
            var settings = LoadSettings();
            if (settings == null)
                Console.WriteLine("Could not start WabbaBot; failed to load settings.");
            else {
                var bot = new Bot(settings);
                await bot.RunAsync();
            }

            await Task.Delay(-1);
        }

        private static BotSettings? LoadSettings() {
            string settingsPath = Path.Combine("Config/", "Settings.json");
            return JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(settingsPath));
        }
    }
}