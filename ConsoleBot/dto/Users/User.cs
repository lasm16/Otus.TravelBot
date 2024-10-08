using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.dto.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public UserType UserType { get; private set; }
        public List<Post> Posts { get; set; }

        public User(Guid id, string name, string email, UserType userType)
        {
            Id = id;
            Name = name;
            Email = email;
            UserType = userType;
        }
    }

    public enum UserType
    {
        SimpleUser,
        Admin
    }
}
