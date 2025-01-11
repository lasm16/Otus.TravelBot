using DataBase;
using DataBase.Models;

namespace TelegramBot.Business.Utils
{
    public class Scheduler
    {
        public static async Task CheckUpdates(CancellationTokenSource cancellationToken)
        {
            using var db = new ApplicationContext();
            var trips = db.Trips.ToList();
            var acceptedTrips = trips.Where(x => x.DateStart == DateTime.Now.Date && x.Status == TripStatus.Accepted);
            var onTheWayTrips = trips.Where(x => x.DateEnd == DateTime.Now.Date && x.Status == TripStatus.OnTheWay);
            if (!acceptedTrips.Any() && !onTheWayTrips.Any())
            {
                return;
            }
            foreach (var trip in acceptedTrips)
            {
                trip.Status = TripStatus.OnTheWay;
                db.Trips.Update(trip);

            }
            foreach (var trip in onTheWayTrips)
            {
                trip.Status = TripStatus.Ended;
                db.Trips.Update(trip);
            }
            await db.SaveChangesAsync(cancellationToken: cancellationToken.Token);
        }
    }
}
