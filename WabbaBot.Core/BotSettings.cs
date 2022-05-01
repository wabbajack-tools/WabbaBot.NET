namespace WabbaBot.Core
{
    public class BotSettings
    {
        public string? Token;
        public string RepositoriesURL = "https://raw.githubusercontent.com/wabbajack-tools/mod-lists/master/repositories.json";
        public int ModlistMetadataCacheTimeout = 300;
        public int ActivityRefreshingTimeout = 3600;
        public HashSet<ulong> Administrators;

    }
}
