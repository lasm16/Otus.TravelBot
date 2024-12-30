using DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBase
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Trip> Trips { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
            if (connectionString == null)
            {
                throw new ArgumentNullException(connectionString, "Не установлена строка подключения к БД в App.config!");
            }
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
