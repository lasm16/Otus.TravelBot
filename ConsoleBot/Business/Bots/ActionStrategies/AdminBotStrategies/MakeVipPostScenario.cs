using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;
using Serilog;

namespace ConsoleBot.Business.Bots.ActionStrategies.AdminBotStrategies
{
    internal class MakeVipPostScenario : IActionStrategy
    {
        private List<Post> _posts = DataRepository.Posts;
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForVip);
            Guid.TryParse(inputLine, out var guid);
            MakePostVip(guid);
        }

        private void MakePostVip(Guid guid)
        {
            var post = _posts.Where(x => x.Id == guid).FirstOrDefault();
            if (post == null)
            {
                Console.WriteLine($"Пост с id = {guid} не найден!");
                return;
            }
            if (post.IsVipRequested == false)
            {
                Console.WriteLine($"Для поста с id = {guid} не было запроса на добавление VIP-статуса!");
                return;
            }

            post.IsVip = true;
            post.IsVipRequested = false;
            Console.WriteLine($"Для поста с id = {guid} был установлен VIP-статус!");
        }
    }
}