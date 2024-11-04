using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using Serilog;

namespace ConsoleBot.Business.Bots.Scenarios.UserBotScenarios
{
    internal class ShowTripsScenario(User user) : IAction
    {
        public void DoAction()
        {
            Console.WriteLine(BotPhrases.FindTrips);
            ShowTrips(user);
        }

        private static void ShowTrips(User user)
        {
            var trips = user.Trips;
            if (trips != null)
            {
                foreach (var post in trips)
                {
                    // Будет вывод на веб-интерфейс
                    Console.WriteLine(post);
                }
            }
            Log.Debug("У вас нет поездок!");
        }
    }
}