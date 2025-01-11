using Microsoft.Extensions.Configuration;

namespace TelegramBot.Business
{
    public static class AppConfig
    {
        private static readonly IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
        private static readonly IConfigurationRoot root = builder.Build();
        public static string? Token => root["AppSettings:UserBotToken"];
        public static List<string> LaunchCommands => GetLaunchCommands();
        public static string? ConnectionString => root["AppSettings:ConnectionString"];


        private static List<string> GetLaunchCommands()
        {
            var keyValuePairs = root.GetSection("AppSettings:LaunchCommands").AsEnumerable().ToDictionary();
            if (keyValuePairs == null)
            {
                return
                [
                    "/start"
                ];
            }
            return [.. keyValuePairs.Values.Where(x => x != null)];
        }
    }
}
