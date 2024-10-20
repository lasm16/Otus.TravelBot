using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies
{
    internal class FindFellowScenario(User user) : IActionStrategy
    {
        private List<User> _users = DataRepository.Users;

        public void DoAction()
        {
            var dateTime = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestDate);
            DateTime.TryParse(dateTime, out DateTime dateTimeOut);
            SearchFellow(user, dateTimeOut);
            Console.WriteLine(BotPhrases.Done);
        }

        private void SearchFellow(User user, DateTime dateTime)
        {
            _users.Remove(user);
            var usersWithPosts = _users.Where(x => x.Posts != null);
            var list = new List<Post>();

            foreach (var userInList in usersWithPosts) // заменить бы чем
            {
                var posts = userInList.Posts;
                foreach (var post in posts)
                {
                    if (post.TravelDateStart == dateTime)
                    {
                        Console.WriteLine($"Автор поста {userInList.LinkTg} \n {post}");
                    }
                }
            }
        }
    }
}