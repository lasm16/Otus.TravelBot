using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.AdminBotStrategies
{
    internal class AcceptPostScenario : IActionStrategy
    {
        private List<Post> _posts = DataRepository.Posts;
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForAccept);
            Guid.TryParse(inputLine, out var guid);
            AcceptPost(guid);
        }

        private void AcceptPost(Guid guid)
        {
            var post = _posts.Where(x => x.Id == guid).FirstOrDefault();
            if (post == null)
            {
                Console.WriteLine($"Пост с id = {guid} не найден!");
                return;
            }
            post.Status = "Запланирована";
            //SendToPublic(post);
        }
    }
}