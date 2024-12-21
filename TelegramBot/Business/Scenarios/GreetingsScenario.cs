using Common.Model;
using Common.Model.Bot;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Business.Bots.Roles;
using TelegramBot.Business.Scenarios.UserScenarios;

namespace TelegramBot.Business.Scenarios
{
    public class GreetingsScenario : IScenario
    {
        private IBotRole? _botRole;
        private readonly TelegramBotClient _botClient;

        public GreetingsScenario(TelegramBotClient botClient)
        {
            _botClient = botClient;
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }

        public void DoAction()
        {
        }

        private async Task OnUpdate(Update update)
        {
            var action = update.CallbackQuery.Data;
            var message = update.CallbackQuery.Message;
            var scenario = _botRole!.Actions.FirstOrDefault(t => t.Key == action).Value;
            _botClient.OnError -= OnError;
            _botClient.OnMessage -= OnMessage;
            _botClient.OnUpdate -= OnUpdate;
            scenario.DoAction();
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
                await _botClient!.SendMessage(message.Chat.Id, $"Я не знаю этой команды...");
                return;
            }

            var currentUser = message.From!.FirstName + " " + message.From.LastName;
            var greetingsText = $"Приветствую тебя, {currentUser}! Ты можешь выложить пост о планируемой поездке или найти попутчика.";

            var actions = _botRole.Actions.Keys;
            var inlineMarkup = new InlineKeyboardMarkup();
            foreach (var item in actions)
            {
                inlineMarkup.AddButton(item, item);
            }

            await _botClient!.SendMessage(message.Chat.Id, greetingsText!, replyMarkup: inlineMarkup);
        }

        private void CheckRole(Message message)
        {
            var userId = message.From!.Id;
            var user = new Common.Model.User()
            {
                Id = userId,
                UserType = UserType.SimpleUser
            };
            if (_botRole != null)
            {
                return;
            }
            if (UserType.Admin == user.UserType)
            {
                _botRole = new AdminRole
                {
                    Actions = new Dictionary<string, IScenario>
                    {
                        { "/start", new GreetingsScenario(_botClient) }
                    }
                };
            }
            else
            {
                _botRole = new UserRole
                {
                    Actions = new Dictionary<string, IScenario>
                    {
                        //{ "/start", new GreetingsScenario(_botClient) },
                        { "Новая поездка", new CreateNewTripScenario(_botClient) }
                    }
                };
            }
        }
    }
}
