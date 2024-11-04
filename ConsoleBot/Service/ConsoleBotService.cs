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

        public Dictionary<string, IAction> AvailibleActions = [];

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

        public void Greeting()
        {
            var message = _bot.GreetingMessage;
            Console.WriteLine(message);
        }

        public void LaunchScenario(IAction action)
        {
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
                var value = _bot.Actions[i];
                var key = _userScenarios[i];
                AvailibleActions.Add(key, value);
            }
        }

        private void FillDictionaryWithAdminScenarios()
        {
            var count = _bot.Actions.Count;
            for (var i = 0; i < count; i++)
            {
                var value = _bot.Actions[i];
                var key = _adminScenarios[i];
                AvailibleActions.Add(key, value);
            }
        }
    }
}
