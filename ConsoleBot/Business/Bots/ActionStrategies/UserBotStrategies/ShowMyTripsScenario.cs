using Common.Bot;
using Common.Model;
using Common.Model.Bot;

namespace ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies
{
    internal class ShowMyTripsScenario(User user) : IActionStrategy
    {
        public void DoAction()
        {
            Console.WriteLine(BotPhrases.FindTrips);
            FindAllPosts(user);
        }

        private void FindAllPosts(User user)
        {
            var posts = user.Posts;
            if (posts != null)
            {
                foreach (var post in posts)
                {
                    Console.WriteLine(post);
                }
            }
            Console.WriteLine("У вас нет постов!");
        }
    }
}