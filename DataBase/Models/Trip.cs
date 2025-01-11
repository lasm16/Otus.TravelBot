namespace DataBase.Models
{
    public class Trip
    {
        public Guid Id { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Description { get; set; }
        public string? Photo { get; set; }
        public TripStatus Status { get; set; }
        public long UserId { get; set; }
        public User? User { get; set; }
    }

    public enum TripStatus
    {
        New,
        Accepted,
        Declined,
        OnTheWay,
        Ended
    }
}
