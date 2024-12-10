using Common.Model.Bot;
using Common.Services;

namespace TelegramBot.Services
{
    public class TelegramBotService(IBot bot) : IBotService
    {
        public async Task StartAsync()
        {
            await bot.CreateBotAsync();
        }
    }
}
