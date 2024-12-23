using Common.Services;
using Telegram.Bot;
using TelegramBot.Business.Scenarios;

namespace TelegramBot.Services
{
    public class TelegramBotService() : IBotService
    {
        private TelegramBotClient? _botClient;
        private readonly string? _key = System.Configuration.ConfigurationManager.AppSettings["userBotToken"];

        public async Task StartBotAsync()
        {
            using var cts = new CancellationTokenSource();
            if (_key == null || string.Empty.Equals(_key))
            {
                throw new ArgumentNullException(_key, "Не установлен токен в App.config!");
            }
            _botClient = new TelegramBotClient(_key, cancellationToken: cts.Token);
            var me = await _botClient.GetMe();

            var scenario = new GreetingsScenario(_botClient);
            scenario.Launch();

            Console.WriteLine($"@{me!.Username} is running... Press Esc to terminate");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            cts.Cancel();
        }
    }
}
