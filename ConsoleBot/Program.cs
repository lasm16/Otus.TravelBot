using ConsoleBot.Bots;
using ConsoleBot.dto;
using ConsoleBot.dto.Users;
using ConsoleBot.service;

namespace ConsoleBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            User user;
            IBot bot;

            Console.WriteLine(BotPhrases.Welcome);
            Console.WriteLine(BotPhrases.Role);
            var role = Console.ReadLine();

            if (role.Equals("пользователь"))
            {
                user = new User(new Guid(), "Вася", "Ololoev1", UserType.SimpleUser);
                bot = new UserBot(user);
            }
            else
            {
                user = new User(new Guid(), "Петя", "PrettyBitch", UserType.Admin);
                bot = new AdminBot();
            }

            var service = new ConsoleBotService(bot);
            service.Greeting();

            var actions = service.Actions; 
            Console.WriteLine(BotPhrases.AvailableActions);
            foreach (var action in actions)
            {
                Console.WriteLine(action);
            }
            var userAction = Console.ReadLine();
            service.LauchScenario(userAction);
        }
    }
}
