namespace Common.Model
{
    public class Trip(Guid id, DateTime dateTimeStart, DateTime dateTimeEnd, string discription, byte[]? picture)
    {
        public Guid Id { get; private set; } = id;
        public DateTime DateStart { get; set; } = dateTimeStart;
        public DateTime DateEnd { get; set; } = dateTimeEnd;
        public string Description { get; set; } = discription;
        public byte[]? Picture { get; set; } = picture;
        public TripStatus Status { get; set; } = TripStatus.Review;
    }

    public enum TripStatus
    {
        Review,
        Planing,
        OnTheWay,
        Ended
    }
}
