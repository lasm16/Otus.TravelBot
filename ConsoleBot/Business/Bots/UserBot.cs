using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Bots.Scenarios.UserBotScenarios;

namespace ConsoleBot.Business.Bots
{
    public class UserBot(User user) : IBot
    {
        private User _currentUser = user;

        public Dictionary<string, IAction> Actions => new()
        {
            { "Создать новую поездку",  new CreateNewTripScenario(_currentUser) },
            { "Найти попутчика",        new FindFellowScenario(_currentUser) },
            { "Мои поездки",            new ShowTripsScenario(_currentUser) },
            { "Редактировать поездку",  new UpdateTripScenario(_currentUser) },
            { "Удалить поездку",        new DeleteTripScenario(_currentUser) },
        };
        public string GreetingMessage => $"Приветствую тебя, {_currentUser.Name}! Ты можешь выложить пост о планируемой поездке или найти попутчика";
    }
}
