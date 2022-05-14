namespace WabbaBot
{
    public class BotSettings
    {
        public string? Token;
        public int ModlistMetadataCacheTimeout = 300;
        public int ActivityRefreshingTimeout = 3600;
        public HashSet<ulong>? Administrators;

    }
}
