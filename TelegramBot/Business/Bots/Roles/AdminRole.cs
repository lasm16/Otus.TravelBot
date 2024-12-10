using Common.Model.Bot;
using Telegram.Bot.Types;
using TelegramBot.Business.Scenarios;

namespace TelegramBot.Business.Bots.Roles
{
    internal class AdminRole(Message message) : IBotRole
    {
        private Message _message = message;

        public Dictionary<string, IScenario> Actions => new()
        {
            { "/start",  new GreetingsScenario(_message) },
            //{ "Новые посты",  new ShowNewTripsScenario() },
            //{ "Принять",      new AcceptTripScenario() },
            //{ "Отклонить",    new DeclineTripScenario() },
        };
    }
}
