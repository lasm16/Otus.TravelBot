using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Bots.ActionStrategies.UserBotStrategies
{
    public class DeletePostScenario : IActionStrategy
    {
        public void DoAction(IUser user)
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForDelete);
            Guid.TryParse(inputLine, out var guid);
            DeletePost(user, guid);
        }

        private void DeletePost(IUser user, Guid guid)
        {
            var post = user.Posts.FirstOrDefault(x => x.Id == guid);
            if (post != null)
            {
                user.Posts.Remove(post);
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}