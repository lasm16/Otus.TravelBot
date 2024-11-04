using Common.Model;

namespace ConsoleBot.Business.Repositories
{
    internal class DataRepository
    {
        public static List<Trip>? Trips { get; set; }
        public static List<User>? Users { get; set; }
    }
}
