using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.Scenarios.UserBotScenarios
{
    public class FindFellowScenario(User user) : IAction
    {
        private List<User> _users = DataRepository.Users;

        public void DoAction()
        {
            var dateTime = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestDate);
            _ = DateTime.TryParse(dateTime, out DateTime dateTimeOut);
            SearchFellow(user, dateTimeOut);
            Console.WriteLine(BotPhrases.Done);
        }

        private void SearchFellow(User user, DateTime dateTime)
        {
            _users.Remove(user);
            var usersWithTrips = _users.Where(x => x.Trips != null);
            var list = new List<Trip>();

            foreach (var userInList in usersWithTrips) // заменить бы чем
            {
                var trips = userInList.Trips;
                foreach (var trip in trips)
                {
                    if (trip.DateStart == dateTime && trip.Status.Equals(TripStatus.Planing))
                    {
                        Console.WriteLine($"Автор поездки {userInList.LinkTg} \n {trip}");
                    }
                }
            }
        }
    }
}