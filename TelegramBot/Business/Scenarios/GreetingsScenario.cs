using Common.Data;
using Common.Model;
using Common.Model.Bot;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Business.Bots.Roles;
using TelegramBot.Business.Scenarios.AdminScenarios;
using TelegramBot.Business.Scenarios.UserScenarios;

namespace TelegramBot.Business.Scenarios
{
    public class GreetingsScenario(TelegramBotClient botClient) : IScenario
    {
        private IBotRole? _botRole;
        private readonly TelegramBotClient _botClient = botClient;

        //заменить на инициализацию в конструкторе?
        public void Launch()
        {
            SubscriveEvents();
        }

        private void SubscriveEvents()
        {
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }

        private async Task OnUpdate(Update update)
        {
            if (update.MessageReaction != null)
            {
                return;
            }
            var scenario = GetScenario(update);
            await RemoveInlineKeyboard(update);
            UnsubscribeEvents();
            scenario.Launch();
        }

        private IScenario GetScenario(Update update)
        {
            var action = update.CallbackQuery!.Data;
            // кидает NRE после нажатия кнопки "Готово" у подтверждения поездки
            return _botRole!.Actions!.FirstOrDefault(t => t.Key == action).Value;
        }

        private async Task RemoveInlineKeyboard(Update update)
        {
            var chatId = update.CallbackQuery!.Message!.Chat.Id;
            var messageId = update.CallbackQuery.Message.Id;
            await _botClient.EditMessageReplyMarkup(chatId, messageId, null);
        }

        private void UnsubscribeEvents()
        {
            _botClient.OnError -= OnError;
            _botClient.OnMessage -= OnMessage;
            _botClient.OnUpdate -= OnUpdate;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text is null)
            {
                return;
            }
            CheckRole(message);
            var action = message.Text;
            if (!action.Equals("/start"))
            {
                Log.Error("Некорректно указан сценарий!");
                await _botClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
                return;
            }

            var currentUser = message.From!.FirstName + " " + message.From.LastName;
            var greetingsText = $"Приветствую тебя, {currentUser}! Ты можешь выложить пост о планируемой поездке или найти попутчика.";

            var actions = _botRole!.Actions!.Keys;
            var inlineMarkup = new InlineKeyboardMarkup();
            foreach (var item in actions)
            {
                inlineMarkup.AddButton(item, item);
            }
            var chatId = message.Chat.Id;
            await _botClient.SendMessage(chatId, greetingsText!, replyMarkup: inlineMarkup);
        }

        private void CheckRole(Message message)
        {
            if (_botRole != null)
            {
                return;
            }
            var tgUser = message.From;
            var user = GetUser(tgUser!); // здесь должна быть проверка роли админ/юзер через значение в бд
            if (UserType.Admin == user.UserType)
            {
                SetAdminActions();
            }
            else if (UserType.SimpleUser == user.UserType)
            {
                SetUserActions();
            }
        }

        private Common.Model.User GetUser(Telegram.Bot.Types.User user)
        {
            var nickName = user.Username;
            if (nickName == System.Configuration.ConfigurationManager.AppSettings["adminNickname"])
            {
                return new Common.Model.User()
                {
                    Id = user.Id,
                    UserType = UserType.Admin
                };
            }
            return new Common.Model.User()
            {
                Id = user.Id,
                UserType = UserType.SimpleUser
            };
        }

        private void SetUserActions()
        {
            _botRole = new UserRole
            {
                Actions = new Dictionary<string, IScenario>
                    {
                        { "Новая поездка",      new CreateNewTripScenario(_botClient) },
                        { "Мои поездки",        new ShowTripsScenario(_botClient) },
                        { "Найти попутчика",    new FindFellowScenario(_botClient) }
                    }
            };
        }

        private void SetAdminActions()
        {
            _botRole = new AdminRole
            {
                Actions = new Dictionary<string, IScenario>
                    {
                        { "Новые посты",    new ShowNewTripsScenario(_botClient) },
                    }
            };
        }
    }
}
