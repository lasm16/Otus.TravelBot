using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;

namespace ConsoleBot.Business.Bots.ActionStrategies.AdminBotStrategies
{
    internal class ShowPublishedPostsScenario : IActionStrategy
    {
        private List<Post> _posts = DataRepository.Posts;
        public void DoAction()
        {
            ShowPublishedPosts();
        }

        private void ShowPublishedPosts()
        {
            var newPosts = _posts.Where(x => x.Status.Equals("Запланирована"));
            foreach (var post in newPosts)
            {
                Console.WriteLine(post);
            }
        }
    }
}