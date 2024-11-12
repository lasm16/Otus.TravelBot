using Common.Model.Bot;
using ConsoleBot.Business.Bots.Scenarios.AdminBotScenarios;

namespace ConsoleBot.Business.Bots
{
    public class AdminBot() : IBot
    {
        public Dictionary<string, IAction> Actions => new()
        {
            { "Новые посты",  new ShowNewTripsScenario() },
            { "Принять",      new AcceptTripScenario() },
            { "Отклонить",    new DeclineTripScenario() },
        };
        public string GreetingMessage => "Добрый день, админ. Что будем делать?";
    }
}
