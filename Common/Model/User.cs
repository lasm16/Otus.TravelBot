namespace Common.Model
{
    public class User(Guid id, string name, string linkTg, UserType userType)
    {
        public Guid Id { get; private set; } = id;
        public string Name { get; private set; } = name;
        public string LinkTg { get; private set; } = linkTg;
        public UserType UserType { get; private set; } = userType;
        public List<Trip> Trips { get; set; }
    }

    public enum UserType
    {
        SimpleUser,
        Admin
    }
}
