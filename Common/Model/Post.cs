namespace Common.Model
{
    public class Post
    {
        public User? User { get; set; }
        public List<Trip>? Trips { get; set; }
    }
}
