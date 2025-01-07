using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataBase
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Trip> Trips { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfigurationRoot root = builder.Build();
            var connectionString = root["AppSettings:ConnectionString"];

            if (connectionString == null)
            {
                throw new ArgumentNullException(connectionString, "Не установлена строка подключения к БД в appsettings.json!");
            }
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
