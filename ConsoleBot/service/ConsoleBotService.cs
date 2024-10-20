using Common.Model.Bot;
using Common.Services;
using Serilog;

namespace ConsoleBot.Service
{
    public class ConsoleBotService(IBot bot) : IBotService
    {
        private IBot _bot = bot;

        public IList<string> Actions => _bot.AvailableActions;

        public void Greeting()
        {
            var message = _bot.SendGreetingMessage();
            Console.WriteLine(message);
        }

        public void LaunchScenario(string scenario)
        {
            Log.Debug($"Запускаем сценарий для бота: {scenario}");
            _bot.PerfomAction(scenario);
        }
    }
}
