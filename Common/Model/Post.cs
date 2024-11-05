namespace Common.Model
{

    /// <summary>
    /// Depricated
    /// Пост связан с поездкой, но пока не стоит трогать, чтобы не усложнять логику
    /// </summary>
    public class Post
    {
        public Guid Id { get; set; }
        public Trip Trip { get; set; }
        public VipStatus Status { get; set; }

        public Post(Guid id, DateTime dateTimeStart, DateTime dateTimeEnd, string discription, byte[] picture)
        {
            var trip = new Trip(Guid.NewGuid(), dateTimeStart, dateTimeEnd, discription, picture);

            Id = id;
            Trip = trip;
            Trip.Status = TripStatus.Review;
        }
    }

    public enum VipStatus
    {
        Requested,
        Vip,
        Regular
    }
}
