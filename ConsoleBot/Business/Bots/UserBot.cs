using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Bots.ActionStrategies;
using ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies;

namespace ConsoleBot.Business.Bots
{
    public class UserBot : IBot
    {
        private User _currentUser;
        private Dictionary<string, IActionStrategy> actionWithStrategyDictionary;

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
            strategy.DoAction();
        }

        private void FillDictionaryWithStratagies()
        {
            actionWithStrategyDictionary = new Dictionary<string, IActionStrategy>
            {
                { "Создать новую поездку",  new NewTripScenario(_currentUser) },
                { "Найти попутчика",        new FindFellowScenario(_currentUser) },
                { "Мои поездки",            new ShowMyTripsScenario(_currentUser) },
                { "Редактировать пост",     new UpdatePostScenario(_currentUser) },
                { "Удалить пост",           new DeletePostScenario(_currentUser) },
                { "Запрос на VIP-пост",     new RequestVipForPostScenario(_currentUser) }
            };
        }
    }
}
