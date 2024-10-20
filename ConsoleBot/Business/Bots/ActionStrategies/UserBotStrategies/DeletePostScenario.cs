using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies
{
    public class DeletePostScenario(User user) : IActionStrategy
    {
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForDelete);
            Guid.TryParse(inputLine, out var guid);
            DeletePost(user, guid);
        }

        private void DeletePost(User user, Guid guid)
        {
            var post = user.Posts.FirstOrDefault(x => x.Id == guid);
            if (post != null)
            {
                user.Posts.Remove(post);
                return;
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}