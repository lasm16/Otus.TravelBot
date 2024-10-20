using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Bots.ActionStrategies.UserBotStrategies
{
    internal class RequestVipForPostScenario : IActionStrategy
    {
        public void DoAction(IUser user)
        {
            Console.WriteLine(BotPhrases.PostForVip);

            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForVip);
            Guid.TryParse(inputLine, out var guid);
            RequestVipFotPost(user, guid);
        }

        private void RequestVipFotPost(IUser user, Guid guid)
        {
            var post = user.Posts.FirstOrDefault(x => x.Id == guid);
            if (post != null)
            {
                post.IsVipRequested = true;
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}