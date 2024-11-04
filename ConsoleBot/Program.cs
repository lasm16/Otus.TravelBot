using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Bots;
using ConsoleBot.Data;
using ConsoleBot.Data.Logger;
using ConsoleBot.Service;
using Serilog;

namespace ConsoleBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            User user;
            IBot bot;
            Logger.GetLogger();

            Console.WriteLine(BotPhrases.Welcome);
            var role = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.Role);
            if (role.Equals("пользователь"))
            {
                user = new User(Guid.NewGuid(), "Вася", "Ololoev1", UserType.SimpleUser);
                bot = new UserBot(user);
            }
            else
            {
                user = new User(Guid.NewGuid(), "Петя", "PrettyBitch", UserType.Admin);
                bot = new AdminBot();
            }

            Log.Debug($"В систему вошел пользователь: {user.Id}, {user.Name}, {user.UserType}");
            var service = new ConsoleBotService(bot);
            service.Greeting();

            var availibleActions = service.AvailibleActions.Keys;
            Console.WriteLine(BotPhrases.AvailableActions + "\n");
            foreach (var availibleAction in availibleActions)
            {
                Console.WriteLine(availibleAction);
            }
            var userAction = Console.ReadLine()!;
            service.AvailibleActions.TryGetValue(userAction, out var action);
            service.LaunchScenario(action!);
        }
    }
}
