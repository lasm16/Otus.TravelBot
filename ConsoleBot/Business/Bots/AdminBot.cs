using Common.Model.Bot;
using ConsoleBot.Business.Bots.Scenarios.AdminBotScenarios;

namespace ConsoleBot.Business.Bots
{
    public class AdminBot() : IBot
    {
        public List<IAction> Actions =>
        [
            new ShowNewTripsScenario(),
            new AcceptTripScenario(),
            new DeclineTripScenario()
        ];
        public string GreetingMessage => "Добрый день, админ. Что будем делать?";
    }
}
