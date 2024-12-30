namespace TelegramBot.Business
{
    public static class AppConfig
    {
        public static string? Token => System.Configuration.ConfigurationManager.AppSettings["userBotToken"];
        public static List<string> LaunchCommands => GetLaunchCommands();
        public static string? ConnectionString => System.Configuration.ConfigurationManager.AppSettings["connectionString"];


        private static List<string> GetLaunchCommands()
        {
            var list = System.Configuration.ConfigurationManager.AppSettings["launchCommand"];
            if (list == null)
            {
                return
                [
                    "/start"
                ];
            }
            return [.. list.Split([','], StringSplitOptions.RemoveEmptyEntries)];
        }
    }
}
