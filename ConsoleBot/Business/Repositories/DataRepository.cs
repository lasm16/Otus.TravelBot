using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.Business.Repositories
{
    internal class DataRepository
    {
        public static List<Post> Posts { get; set; }
        public static List<User> Users { get; set; }
    }
}
