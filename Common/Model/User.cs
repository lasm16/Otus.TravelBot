namespace Common.Model
{
    public class User: IUser
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string LinkTg { get; private set; }
        public UserType UserType { get; private set; }
        public IList<Post> Posts { get; set; }

        public User(Guid id, string name, string linkTg, UserType userType)
        {
            Id = id;
            Name = name;
            LinkTg = linkTg;
            UserType = userType;
        }
    }

    public enum UserType
    {
        SimpleUser,
        Admin
    }
}
