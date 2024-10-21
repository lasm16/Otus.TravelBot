using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.AdminBotStrategies
{
    internal class DeclinePostScenario : IActionStrategy
    {
        private List<Post> _posts = DataRepository.Posts;
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForDecline);
            Guid.TryParse(inputLine, out var guid);
            DeclinePost(guid);
        }

        private void DeclinePost(Guid guid)
        {
            var post = _posts.Where(x => x.Id == guid).FirstOrDefault();
            if (post == null)
            {
                Console.WriteLine($"Пост с id = {guid} не найден!");
                return;
            }
            post.Status = "Отклонена";
        }
    }
}