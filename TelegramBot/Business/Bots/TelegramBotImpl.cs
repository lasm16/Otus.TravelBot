using Serilog;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Common.Model.Bot;
using Common.Model;
using TelegramBot.Business.Bots.Roles;

namespace TelegramBot.Business.Bots
{
    public class TelegramBotImpl : IBot
    {
        private IBotRole? _botRole;
        private TelegramBotClient? _client;
        private readonly string? _key = System.Configuration.ConfigurationManager.AppSettings["userBotToken"];

        public static string? UserName { get; private set; }
        public async Task CreateBotAsync()
        {
            using var cts = new CancellationTokenSource();
            if (_key == null || string.Empty.Equals(_key))
            {
                throw new ArgumentNullException(_key, "Не установлен токен в App.config!");
            }
            _client = new TelegramBotClient(_key, cancellationToken: cts.Token);
            var me = await _client.GetMe();
            _client.OnMessage += OnMessage;
            _client.OnError += OnError;
            _client.OnUpdate += OnUpdate;

            Console.WriteLine($"@{me!.Username} is running... Press Esc to terminate");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            cts.Cancel();
        }

        private async Task OnUpdate(Update update)
        {
            var action = update.CallbackQuery.Data;
            var message = update.CallbackQuery.Message;
            CheckRole(message);
            var scenario = _botRole!.Actions.FirstOrDefault(t => t.Key == action).Value;
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
            var scenario = _botRole!.Actions.FirstOrDefault(t => t.Key == action).Value;
            if (scenario == null)
            {
                Log.Error("Некорректно указан сценарий!");
                await _client!.SendMessage(message.Chat.Id, $"Я не знаю этой команды...");
                return;
            }

            scenario.DoAction();
            var text = scenario.Text;
            if (scenario.InlineKeyboard != null)
            {
                await _client!.SendMessage(message.Chat.Id, text!, replyMarkup: scenario.InlineKeyboard);
            }
            else
            {
                await _client!.SendMessage(message.Chat.Id, text!);
            }
        }

        // переделать!!!
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
                _botRole = new AdminRole(message);
            }
            else
            {
                _botRole = new UserRole(message);
            }
        }
    }
}
