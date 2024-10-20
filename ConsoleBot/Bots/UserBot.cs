using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Bots.ActionStrategies.UserBotStrategies;

namespace ConsoleBot.Bots
{
    public class UserBot : IBot
    {
        private User _currentUser;
        private Dictionary<string, IActionStrategy> actionWithStrategyDictionary;

        public IList<Post> Posts => _currentUser.Posts;
        public IList<string> AvailableActions => [.. actionWithStrategyDictionary.Keys];

        public UserBot(User user)
        {
            _currentUser = user;
            FillDictionaryWithStratagies();
        }

        public string SendGreetingMessage() => $"Приветствую тебя, {_currentUser.Name}! Ты можешь выложить пост о планируемой поездке или найти попутчика";
        public void PerfomAction(string action)
        {
            var strategy = actionWithStrategyDictionary[action];
            strategy.DoAction(_currentUser);
        }

        private void FillDictionaryWithStratagies()
        {
            actionWithStrategyDictionary = new Dictionary<string, IActionStrategy>
            {
                { "Создать новую поездку",  new NewTripScenario() },
                { "Найти попутчика",        new FindFellowScenario() },
                { "Мои поездки",            new ShowMyTripsScenario() },
                { "Редактировать пост",     new UpdatePostScenario() },
                { "Удалить пост",           new DeletePostScenario() },
                { "Запрос на VIP-пост",     new RequestVipForPostScenario() }
            };
        }
    }
}
