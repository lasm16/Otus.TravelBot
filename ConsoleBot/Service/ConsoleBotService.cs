using Common.Bot;
using Common.Model.Bot;
using Common.Services;
using ConsoleBot.Business.Bots;
using Serilog;

namespace ConsoleBot.Service
{
    public class ConsoleBotService : IBotService
    {
        private List<string> _userScenarios =
        [
            "Создать новую поездку",
            "Найти попутчика",
            "Мои поездки",
            "Редактировать поездку",
            "Удалить поездку"
        ];
        private List<string> _adminScenarios =
        [
            "Новые посты",
            "Принять",
            "Отклонить"
        ];
        private IBot _bot;

        private Dictionary<string, IAction> _availableActions = [];

        public ConsoleBotService(IBot bot)
        {
            _bot = bot;
            if (_bot is UserBot)
            {
                FillDictionaryWithUserScenarios();
            }
            else
            {
                FillDictionaryWithAdminScenarios();
            }
        }

        public void Start()
        {
            Greeting();

            var availibleActions = _availableActions.Keys;
            Console.WriteLine(BotPhrases.AvailableActions + "\n");
            foreach (var availibleAction in availibleActions)
            {
                Console.WriteLine(availibleAction);
            }

            var userAction = Console.ReadLine()!;
            _availableActions.TryGetValue(userAction, out var action);
            var scenario = _bot.Actions.Find(x => x.Equals(action));
            if (scenario == null)
            {
                Log.Error("Некорректно указан сценарий!");
                return;
            }
            Log.Debug($"Запускаем сценарий для бота: {scenario}");
            scenario.DoAction();
        }

        //Может поменять на что-то другое?
        private void FillDictionaryWithUserScenarios()
        {
            var count = _bot.Actions.Count;
            for (var i = 0; i < count; i++)
            {
                var key = _userScenarios[i];
                var value = _bot.Actions[i];
                _availableActions.Add(key, value);
            }
        }

        private void FillDictionaryWithAdminScenarios()
        {
            var count = _bot.Actions.Count;
            for (var i = 0; i < count; i++)
            {
                var key = _adminScenarios[i];
                var value = _bot.Actions[i];
                _availableActions.Add(key, value);
            }
        }

        private void Greeting()
        {
            var message = _bot.GreetingMessage;
            Console.WriteLine(message);
        }
    }
}
