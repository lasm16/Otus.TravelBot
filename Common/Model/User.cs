namespace Common.Model
{
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public UserType UserType { get; set; }
    }

    public enum UserType
    {
        SimpleUser,
        Admin
    }
}
