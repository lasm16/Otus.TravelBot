using Common.Bot;
using Common.Model.Bot;
using Common.Services;
using Serilog;

namespace ConsoleBot.Service
{
    public class ConsoleBotService(IBot bot) : IBotService
    {
        private IBot _bot = bot;

        public void Start()
        {
            Greeting();

            Console.WriteLine(BotPhrases.AvailableActions + "\n");

            var actions = _bot.Actions;
            foreach (var availableAction in actions.Keys)
            {
                Console.WriteLine(availableAction);
            }

            var userAction = Console.ReadLine()!;
            actions.TryGetValue(userAction, out var action);
            var scenario = actions.Values.Where(x => x.Equals(action)).FirstOrDefault();
            if (scenario == null)
            {
                Log.Error("Некорректно указан сценарий!");
                return;
            }
            Log.Debug($"Запускаем сценарий для бота: {scenario}");
            scenario.DoAction();
        }

        private void Greeting()
        {
            var message = _bot.GreetingMessage;
            Console.WriteLine(message);
        }
    }
}
