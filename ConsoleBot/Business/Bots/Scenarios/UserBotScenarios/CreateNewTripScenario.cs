using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies
{
    internal class CreateNewTripScenario(User user) : IAction
    {
        public void DoAction()
        {
            Console.WriteLine(BotPhrases.Agreement);

            var dateTimeStartString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestStartDate);
            _ = DateTime.TryParse(dateTimeStartString, out var startDate);

            var dateTimeEndString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestEndDate);
            _ = DateTime.TryParse(dateTimeEndString, out var endDate);

            var description = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.Description);
            byte[]? picture = null; // заглушка

            CreateNewTrip(user, startDate, endDate, description, picture);
            Console.WriteLine(BotPhrases.Done);
        }

        private static void CreateNewTrip(User user, DateTime dateTimeStart, DateTime dateTimeEnd, string description, byte[]? picture)
        {
            var trip = new Trip(new Guid(), dateTimeStart, dateTimeEnd, description, picture);
            user.Trips.Add(trip);
        }
    }
}
