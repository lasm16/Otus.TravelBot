namespace TelegramBot.Business
{
    public static class AppConfig
    {
        public static string? Token => System.Configuration.ConfigurationManager.AppSettings["userBotToken"];
        public static string? AdminName => System.Configuration.ConfigurationManager.AppSettings["adminNickname"];
        public static string? LaunchCommand => System.Configuration.ConfigurationManager.AppSettings["launchCommand"];
    }
}
