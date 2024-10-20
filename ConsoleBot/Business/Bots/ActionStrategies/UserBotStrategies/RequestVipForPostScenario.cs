using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies
{
    internal class RequestVipForPostScenario(User user) : IActionStrategy
    {
        public void DoAction()
        {
            Console.WriteLine(BotPhrases.PostForVip);

            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForVip);
            Guid.TryParse(inputLine, out var guid);
            RequestVipFotPost(user, guid);
        }

        private void RequestVipFotPost(User user, Guid guid)
        {
            var post = user.Posts.FirstOrDefault(x => x.Id == guid);
            if (post != null)
            {
                post.IsVipRequested = true;
                return;
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}