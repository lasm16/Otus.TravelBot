namespace Common.Model
{
    public class User
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? LinkTg { get; set; }
        public UserType UserType { get; set; }
        public List<Trip>? Trips { get; set; }
    }

    public enum UserType
    {
        SimpleUser,
        Admin
    }
}
