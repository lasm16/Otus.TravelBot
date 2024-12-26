using Common.Data;
using Common.Model;
using Common.Model.Bot;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Business.Bot.Roles;
using TelegramBot.Business.Scenarios.AdminScenarios;
using TelegramBot.Business.Scenarios.UserScenarios;

namespace TelegramBot.Business.Scenarios
{
    public class GreetingScenario(TelegramBotClient botClient) : IScenario
    {
        private IRole _role;
        private Common.Model.User _user;
        private readonly TelegramBotClient _botClient = botClient;
        private string _launchCommand = System.Configuration.ConfigurationManager.AppSettings["launchCommand"];

        //заменить на инициализацию в конструкторе?
        public void Launch()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }

        private async Task OnUpdate(Update update)
        {
            var scenario = GetScenario(update);
            await RemoveInlineKeyboard(update);
            UnsubscribeEvents();
            scenario.Launch();
        }

        private IScenario GetScenario(Update update)
        {
            var action = update.CallbackQuery!.Data;
            // кидает NRE после нажатия кнопки "Готово" у подтверждения поездки
            return _role!.Actions!.FirstOrDefault(t => t.Key == action).Value;
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
                await _botClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
                return;
            }

            var currentUser = message.From!.FirstName + " " + message.From.LastName;
            var greetingsText = $"Приветствую тебя, {currentUser}! Ты можешь выложить пост о планируемой поездке или найти попутчика.";

            var actions = _role!.Actions!.Keys;
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
            if (_role != null)
            {
                return;
            }
            var tgUser = message.From;
            _user = GetUser(tgUser!); // здесь должна быть проверка роли админ/юзер через значение в бд
            if (UserType.Admin == _user.UserType)
            {
                SetAdminActions();
            }
            else if (UserType.SimpleUser == _user.UserType)
            {
                SetUserActions();
            }
        }

        private static Common.Model.User GetUser(Telegram.Bot.Types.User user)
        {
            var nickName = user.Username;
            if (nickName == System.Configuration.ConfigurationManager.AppSettings["adminNickname"])
            {
                return new Common.Model.User()
                {
                    Id = user.Id,
                    UserName = user.Username,
                    UserType = UserType.Admin
                };
            }
            return new Common.Model.User()
            {
                Id = user.Id,
                UserName = user.Username,
                UserType = UserType.SimpleUser
            };
        }

        private void SetUserActions()
        {
            _role = new UserRole
            {
                Actions = new Dictionary<string, IScenario>
                {
                    { "Новая поездка",      new CreateNewTripScenario(_botClient, _user) },
                    { "Мои поездки",        new ShowMyTripsScenario(_botClient, _user) },
                    { "Найти попутчика",    new FindFellowScenario(_botClient, _user) }
                }
            };
        }

        private void SetAdminActions()
        {
            _role = new AdminRole
            {
                Actions = new Dictionary<string, IScenario>
                {
                    { "Новые посты",        new ShowNewPostsScenario(_botClient) },
                    { "Новая поездка",      new CreateNewTripScenario(_botClient, _user) },
                    { "Мои поездки",        new ShowMyTripsScenario(_botClient, _user) },
                    { "Найти попутчика",    new FindFellowScenario(_botClient, _user) }
                }
            };
        }
    }
}
