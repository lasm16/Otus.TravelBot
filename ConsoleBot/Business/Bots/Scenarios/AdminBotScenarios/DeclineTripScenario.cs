using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;
using Serilog;

namespace ConsoleBot.Business.Bots.Scenarios.AdminBotScenarios
{
    internal class DeclineTripScenario : IAction
    {
        private List<Trip> _trips = DataRepository.Trips;
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForDecline);
            _ = Guid.TryParse(inputLine, out var guid);
            DeclineTrip(guid);
        }

        private void DeclineTrip(Guid guid)
        {
            var trip = _trips.Where(x => x.Id == guid).FirstOrDefault();
            if (trip == null)
            {
                Log.Debug($"Поездка с id = {guid} не найдена!");
                return;
            }
            _trips.Remove(trip);
            Log.Information($"Удалена поездка с id = {guid}");
        }
    }
}