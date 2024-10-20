using Common.Bot;
using Common.Model;
using Common.Model.Bot;

namespace ConsoleBot.Bots.ActionStrategies.UserBotStrategies
{
    internal class ShowMyTripsScenario : IActionStrategy
    {
        public void DoAction(IUser user)
        {
            Console.WriteLine(BotPhrases.FindTrips);
            FindAllPosts(user);
        }

        private void FindAllPosts(IUser user)
        {
            var posts = user.Posts;
            foreach (var post in posts)
            {
                Console.WriteLine(post);
            }
        }
    }
}