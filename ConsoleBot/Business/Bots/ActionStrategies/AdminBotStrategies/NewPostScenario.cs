using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;

namespace ConsoleBot.Business.Bots.ActionStrategies.AdminBotStrategies
{
    internal class NewPostScenario : IActionStrategy
    {
        private List<Post> _posts = DataRepository.Posts;
        public void DoAction()
        {
            ShowNewPosts();
        }

        private void ShowNewPosts()
        {
            var newPosts = _posts.Where(x => x.Status.Equals("Планируется"));
            foreach (var post in newPosts)
            {
                Console.WriteLine(post);
            }
        }
    }
}