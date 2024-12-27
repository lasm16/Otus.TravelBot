using Common.Data;
using Common.Model.Bot;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Business.Scenarios
{
    public class GreetingScenario(TelegramBotClient botClient) : BaseScenario(botClient), IScenario
    {
        private readonly string? _launchCommand = AppConfig.LaunchCommand;

        //заменить на инициализацию в конструкторе?
        public void Launch()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            BotClient.OnError += OnError;
            BotClient.OnMessage += OnMessage;
            BotClient.OnUpdate += OnUpdate;
        }

        private async Task OnUpdate(Update update)
        {
            var scenario = GetScenario(update);
            await RemoveInlineKeyboard(update);
            UnsubscribeEvents();
            scenario.Launch();
        }

        private async Task RemoveInlineKeyboard(Update update)
        {
            var chatId = update.CallbackQuery!.Message!.Chat.Id;
            var messageId = update.CallbackQuery.Message.Id;
            await botClient.EditMessageReplyMarkup(chatId, messageId, null);
        }

        private void UnsubscribeEvents()
        {
            BotClient.OnError -= OnError;
            BotClient.OnMessage -= OnMessage;
            BotClient.OnUpdate -= OnUpdate;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message, exception.StackTrace);
            Log.Debug(exception.Message, exception.StackTrace);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text is null)
            {
                return;
            }
            CheckRole(message);
            var action = message.Text;
            if (!action.Equals(_launchCommand))
            {
                Log.Error("Некорректно указан сценарий!");
                await BotClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
                return;
            }

            var currentUser = message.From!.FirstName + " " + message.From.LastName;
            var greetingsText = BotPhrases.Greeting1 + currentUser + BotPhrases.Greeting2;

            var actions = Role!.Actions!.Keys;
            var inlineMarkup = new InlineKeyboardMarkup();
            foreach (var item in actions)
            {
                inlineMarkup.AddButton(item, item);
            }
            var chatId = message.Chat.Id;
            await BotClient.SendMessage(chatId, greetingsText!, replyMarkup: inlineMarkup);
        }
    }
}
