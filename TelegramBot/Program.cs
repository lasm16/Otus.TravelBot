using TelegramBot.Services;
using TelegramBot.Business.Bots;

namespace TelegramBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var bot = new TelegramBotImpl();
            var service = new TelegramBotService(bot);
            await service.StartAsync();
        }
    }
}
