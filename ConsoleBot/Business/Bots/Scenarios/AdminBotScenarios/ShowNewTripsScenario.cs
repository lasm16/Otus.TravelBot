using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using Serilog;

namespace ConsoleBot.Business.Bots.Scenarios.AdminBotScenarios
{
    internal class ShowNewTripsScenario : IAction
    {
        private List<Trip>? _trips = DataRepository.Trips;
        public void DoAction()
        {
            ShowNewTrips();
        }

        private void ShowNewTrips()
        {
            var newTrips = _trips!.Where(x => x.Status.Equals(TripStatus.Review));
            if (newTrips == null)
            {
                Log.Information("Нет поездок!");
                return;
            }
            foreach (var trip in newTrips)
            {
                // Будет вывод на веб-интерфейс
                Console.WriteLine(trip);
            }
        }
    }
}