using ConsoleBot.dto.Bots;
using ConsoleBot.dto.Users;

namespace ConsoleBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var user = new User(new Guid(), "Вася", "someRandomEmail@randomMail.com", UserType.SimpleUser);
            var bot = new UserBot(user);
            bot.SendGreetingMessage();
        }
    }
}
