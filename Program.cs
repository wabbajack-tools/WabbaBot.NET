using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WabbaBot;
using WabbaBot.Models;

namespace WabbaBot
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
        internal static async Task MainAsync(string[] args) {

            if (args.Length == 3 && args[0] == ("--import-json")) {
                while (true) {
                    Console.WriteLine("You should use this on a fresh database. Are you sure you want to import JSON data (from the old bot)? (Y/N)");
                    var keyPress = Console.ReadKey();
                    if (keyPress.Key == ConsoleKey.Y) {
                        Console.WriteLine("Importing database. This might take a while.");
                        await JsonImporter.ImportJsonDb(args[1], args[2]);
                        Console.WriteLine("Imported database. Press any key to exit.");
                        Console.ReadKey();
                        break;
                    }
                    else if (keyPress.Key == ConsoleKey.N)
                        break;
                }
                return;
            }

            var settings = LoadSettings();
            if (settings == null)
                throw new NullReferenceException("Could not start WabbaBot; failed to load settings.");

            var bot = new Bot(settings, args.Contains("--debug"));
            await bot.RunAsync();

            await Task.Delay(-1);
        }

        private static BotSettings? LoadSettings() => JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(Consts.CONFIG_PATH));
    }
}