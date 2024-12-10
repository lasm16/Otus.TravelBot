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
        private TelegramBotClient _client;
        private readonly string? _key = System.Configuration.ConfigurationManager.AppSettings["userBotToken"];
        private IBotRole _botRole;

        public static string? UserName { get; private set; }
        public async Task CreateBotAsync()
        {
            using var cts = new CancellationTokenSource();
            if (_key == null)
            {
                throw new ArgumentNullException("Не установлен токен в App.config!");
            }
            _client = new TelegramBotClient(_key, cancellationToken: cts.Token);
            var me = await _client.GetMe();
            _client.OnMessage += OnMessage;
            _client.OnError += OnError;
            _client.OnUpdate += OnUpdate;

            Console.WriteLine($"@{me.Username} is running... Press Esc to terminate");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            cts.Cancel(); // stop the bot
        }

        private async Task OnUpdate(Update update)
        {
            throw new NotImplementedException();
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
            var scenario = _botRole.Actions.FirstOrDefault(t => t.Key == action).Value;
            if (scenario == null)
            {
                Log.Error("Некорректно указан сценарий!");
                await _client.SendMessage(message.Chat, $"Некорректно указан сценарий!");
                return;
            }
            scenario.DoAction();
            var text = scenario.Text;
            await _client.SendMessage(message.Chat, text);
        }

        // переделать!!!
        private void CheckRole(Message message)
        {
            var userId = message.From.Id;
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
