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
                user = new User(new Guid(), "Вася", "Ololoev1", UserType.SimpleUser);
                bot = new UserBot(user);
            }
            else
            {
                user = new User(new Guid(), "Петя", "PrettyBitch", UserType.Admin);
                bot = new AdminBot(user);
            }

            Log.Debug($"Пользователь залогинился с ролью {user.UserType}");
            var service = new ConsoleBotService(bot);
            service.Greeting();

            var actions = service.Actions;
            Console.WriteLine(BotPhrases.AvailableActions);
            foreach (var action in actions)
            {
                Console.WriteLine(action);
            }
            var userAction = Console.ReadLine();

            service.LaunchScenario(userAction);
        }
    }
}
