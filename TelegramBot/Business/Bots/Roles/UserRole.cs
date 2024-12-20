using Common.Model.Bot;
using Telegram.Bot.Types;
using TelegramBot.Business.Scenarios;
using TelegramBot.Business.Scenarios.UserScenarios;

namespace TelegramBot.Business.Bots.Roles
{
    public class UserRole(Message message) : IBotRole
    {
        private Message _message = message;

        public Dictionary<string, IScenario> Actions => new()
        {
            { "/start",  new GreetingsScenario(_message) },
            { "Новая поездка",  new CreateNewTripScenario() },
            //{ "Найти попутчика",        new FindFellowScenario(_currentUser) },
            //{ "Мои поездки",            new ShowTripsScenario(_currentUser) },
            //{ "Редактировать поездку",  new UpdateTripScenario(_currentUser) },
            //{ "Удалить поездку",        new DeleteTripScenario(_currentUser) },
        };
    }
}
