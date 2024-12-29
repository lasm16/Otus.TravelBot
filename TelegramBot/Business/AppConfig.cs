namespace TelegramBot.Business
{
    public static class AppConfig
    {
        public static string? Token => System.Configuration.ConfigurationManager.AppSettings["userBotToken"];
        public static string? LaunchCommand => System.Configuration.ConfigurationManager.AppSettings["launchCommand"];
        public static string? ConnectionString => System.Configuration.ConfigurationManager.AppSettings["connectionString"];
    }
}
