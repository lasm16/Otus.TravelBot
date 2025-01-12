using Common.Services;
using Telegram.Bot;
using TelegramBot.Business;
using TelegramBot.Business.Scenarios;
using TelegramBot.Business.Utils;

namespace TelegramBot.Services
{
    public class TelegramBotService() : IBotService
    {
        private TelegramBotClient? _botClient;
        private readonly string? _key = AppConfig.Token;

        public async Task StartBotAsync()
        {
            using var cts = new CancellationTokenSource();
            if (_key == null || string.Empty.Equals(_key))
            {
                throw new ArgumentNullException(_key, "Не установлен токен в appsettings.json");
            }
            _botClient = new TelegramBotClient(_key, cancellationToken: cts.Token);
            var me = await _botClient.GetMe(cancellationToken: cts.Token);

            var scenario = new GreetingScenario(_botClient, null!);
            scenario.Launch();

            Console.WriteLine($"@{me!.Username} is running... Press Esc to terminate");

            //заменить на что-то получше
            while (true)
            {
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                {
                    await Scheduler.CheckUpdates(cts);
                }
            }
        }
    }
}
