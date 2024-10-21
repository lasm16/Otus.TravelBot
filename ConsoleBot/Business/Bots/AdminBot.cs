using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Bots.ActionStrategies;
using ConsoleBot.Business.Bots.ActionStrategies.AdminBotStrategies;

namespace ConsoleBot.Business.Bots
{
    public class AdminBot : IBot
    {
        private User _currentUser;
        private static string _greetingMessage = "Добрый день, админ. Что будем делать?";
        private Dictionary<string, IActionStrategy> actionWithStrategyDictionary;

        public IList<string> AvailableActions => [.. actionWithStrategyDictionary.Keys];

        public AdminBot(User user)
        {
            _currentUser = user;
            FillDictionaryWithStratagies();
        }

        public string SendGreetingMessage() => _greetingMessage;

        public void PerfomAction(string action)
        {
            var strategy = actionWithStrategyDictionary[action];
            strategy.DoAction();
        }

        private void FillDictionaryWithStratagies()
        {
            actionWithStrategyDictionary = new Dictionary<string, IActionStrategy>
            {
                { "Новые посты",            new NewPostScenario() },
                { "Опубликованные посты",   new ShowPublishedPostsScenario() },
                { "Принять",                new AcceptPostScenario() },
                { "Отклонить",              new DeclinePostScenario() },
                { "Сделать VIP-пост",       new MakeVipPostScenario() }
            };
        }
    }
}
