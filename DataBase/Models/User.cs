namespace DataBase.Models
{
    public class User
    {
        public long Id { get; set; }
        public string? NickName { get; set; }
        public int UserTypeId
        {
            get => (int)Type;
            set => Type = (UserType)value;
        }

        public UserType Type { get; set; }

        public List<Trip> Trips { get; set; } = [];
    }

    public enum UserType
    {
        SimpleUser,
        Admin
    }
}
