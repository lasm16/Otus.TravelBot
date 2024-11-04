using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Business.Repositories;
using ConsoleBot.Data;
using Serilog;

namespace ConsoleBot.Business.Bots.Scenarios.AdminBotScenarios
{
    internal class AcceptTripScenario : IAction
    {
        private List<Trip> _trips = DataRepository.Trips;
        public void DoAction()
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForAccept);
            _ = Guid.TryParse(inputLine, out var guid);
            AcceptTrip(guid);
        }

        private void AcceptTrip(Guid guid)
        {
            var trip = _trips.Where(x => x.Id == guid).FirstOrDefault();
            if (trip == null)
            {
                Log.Debug($"Поездка с id = {guid} не найдена!");
                return;
            }
            trip.Status = TripStatus.Planing;
            //SendToPublic(post); уведомление пользователю, что его поездка опубликована
        }
    }
}