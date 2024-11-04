using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Bots.Scenarios.UserBotScenarios;

namespace ConsoleBot.Business.Bots
{
    public class UserBot(User user) : IBot
    {
        private User _currentUser = user;

        public List<IAction> Actions =>
        [
            new CreateNewTripScenario(_currentUser),
            new FindFellowScenario(_currentUser),
            new ShowTripsScenario(_currentUser),
            new UpdateTripScenario(_currentUser),
            new DeleteTripScenario(_currentUser)
        ];
        public string GreetingMessage => $"Приветствую тебя, {_currentUser.Name}! Ты можешь выложить пост о планируемой поездке или найти попутчика";
    }
}
