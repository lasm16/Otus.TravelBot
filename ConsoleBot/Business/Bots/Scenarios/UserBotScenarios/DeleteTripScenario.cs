using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.Scenarios.UserBotScenarios
{
    public class DeleteTripScenario(User user) : IAction
    {
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForDelete);
            _ = Guid.TryParse(inputLine, out var guid);
            DeleteTrip(user, guid);
        }

        private static void DeleteTrip(User user, Guid guid)
        {
            var trip = user.Trips.FirstOrDefault(x => x.Id == guid);
            if (trip != null)
            {
                user.Trips.Remove(trip);
                return;
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}