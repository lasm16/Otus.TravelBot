namespace Common.Model
{
    public class Post
    {
        public User? User { get; set; }
        public List<Trip>? Trips { get; set; }
        //public PostStatus Status { get; set; }
    }

    //public enum PostStatus
    //{
    //    New,
    //    Accepted,
    //    Declined,
    //    OnTheWay,
    //    Ended
    //}
}
