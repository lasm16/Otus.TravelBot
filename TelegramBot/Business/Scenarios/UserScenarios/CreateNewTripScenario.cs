using Common.Data;
using Common.Model.Bot;
using Serilog;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Business.Scenarios.UserScenarios
{
    internal class CreateNewTripScenario(TelegramBotClient botClient) : IScenario
    {
        private TelegramBotClient _botClient = botClient;

        public void DoAction()
        {
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }

        private async Task OnUpdate(Update update)
        {
            var message = new StringBuilder();
            message.Append(BotPhrases.Agreement);
            message.Append(BotPhrases.SuggestStartDate);
            message.Append(BotPhrases.SuggestEndDate);
            message.Append(BotPhrases.Description);
            message.Append(BotPhrases.Photo);
            await _botClient!.SendMessage(update.CallbackQuery.Message.Chat.Id, message.ToString());
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            throw new NotImplementedException();
        }
    }
}
