namespace Common.Model
{
    public class Trip
    {
        public Guid Id { get; set; }
        public string City { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
        public TripStatus Status { get; set; }
    }

    public enum TripStatus
    {
        Review,
        Planing,
        OnTheWay,
        Ended
    }
}
