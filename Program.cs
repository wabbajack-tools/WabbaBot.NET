using Newtonsoft.Json;
using WabbaBot.Core;

namespace WabbaBot
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
        internal static async Task MainAsync(string[] args) {
            var settings = LoadSettings();
            if (settings == null)
                throw new NullReferenceException("Could not start WabbaBot; failed to load settings.");

            var bot = new Bot(settings, args.Contains("--debug"));
            await bot.RunAsync();

            await Task.Delay(-1);
        }

        private static BotSettings? LoadSettings() {
            string settingsPath = Path.Combine("Config/", "Settings.json");
            return JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(settingsPath));
        }
    }
}