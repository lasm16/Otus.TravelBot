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
    public class GreetingScenario : BaseScenario, IScenario
    {
        private List<string> _launchCommands = AppConfig.LaunchCommands;
        private int _messageToDelete = 0;

        public GreetingScenario(TelegramBotClient botClient, DataBase.Models.User user) : base(botClient)
        {
            BotClient = botClient;
            User = user;
        }

        //заменить на инициализацию в конструкторе?
        public void Launch() => SubscribeEvents();


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
            await BotClient.EditMessageReplyMarkup(chatId, messageId, null, cancellationToken: BotClient.GlobalCancelToken);
        }

        private void UnsubscribeEvents()
        {
            BotClient.OnError -= OnError;
            BotClient.OnMessage -= OnMessage;
            BotClient.OnUpdate -= OnUpdate;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            await Task.Run(() =>
            {
                var message = exception.Message;
                var inner = exception.InnerException;
                Console.WriteLine(message);
                Console.WriteLine(inner);
                Log.Debug(exception.Message, exception.StackTrace, exception.InnerException);
            }, cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var inputLine = message.Text ?? message.Photo!.Last().FileId;
            if (inputLine == null)
            {
                return;
            }
            CheckRole(message);
            if (!_launchCommands.Contains(inputLine))
            {
                Log.Error("Некорректно указан сценарий!");
                await BotClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            var tgUser = message.From;
            var userName = GetUserName(tgUser);

            var greetingsText = BotPhrases.Greeting1 + userName + BotPhrases.Greeting2;

            var actions = Role!.Actions!.Keys;
            var inlineMarkup = new InlineKeyboardMarkup();
            foreach (var item in actions)
            {
                inlineMarkup.AddButton(item, item);
            }
            var chatId = message.Chat.Id;
            if (_messageToDelete != 0)
            {
                await BotClient.EditMessageReplyMarkup(chatId, _messageToDelete, null, cancellationToken: BotClient.GlobalCancelToken);
            }
            var messageToDelete = await BotClient.SendMessage(chatId, greetingsText!, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
            _messageToDelete = messageToDelete.Id;
        }

        private static string GetUserName(User? tgUser)
        {
            if (tgUser!.FirstName != null)
            {
                return tgUser.FirstName;
            }
            if (tgUser.Username != null)
            {
                return tgUser.Username;
            }
            return "друг";
        }
    }
}
