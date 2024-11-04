using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.Scenarios.UserBotScenarios
{
    internal class UpdateTripScenario(User user) : IAction
    {
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForUpdate);
            _ = Guid.TryParse(inputLine, out var guid);

            var dateTimeStartString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestStartDate);
            _ = DateTime.TryParse(dateTimeStartString, out var startDate);

            var dateTimeEndString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestEndDate);
            _ = DateTime.TryParse(dateTimeEndString, out var endDate);

            var description = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.Description);

            UpdatePost(user, guid, startDate, endDate, description);
        }

        private static void UpdatePost(User user, Guid guid, DateTime dateTimeStart, DateTime dateTimeEnd, string description)
        {
            var trip = user.Trips.FirstOrDefault(x => x.Id == guid);

            if (trip != null)
            {
                trip.DateStart = dateTimeStart;
                trip.DateEnd = dateTimeEnd;
                trip.Description = description;
                trip.Status = TripStatus.Review;
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}