using Common.Model.Bot;
using Common.Services;

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
            _bot.PerfomAction(scenario);
        }
    }
}
