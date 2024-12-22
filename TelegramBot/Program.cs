using TelegramBot.Services;

namespace TelegramBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = new TelegramBotService();
            await service.StartBotAsync();
        }
    }
}
