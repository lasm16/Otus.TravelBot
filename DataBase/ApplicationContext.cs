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
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            var root = builder.Build();
            var connectionString = root["AppSettings:ConnectionString"];

            if (connectionString == null)
            {
                throw new ArgumentNullException(connectionString, "Не установлена строка подключения к БД в appsettings.json!");
            }
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userList = GetInitialUsers();
            var tripList = GetInitialTrips();
            modelBuilder.Entity<User>().HasData(userList);
            modelBuilder.Entity<Trip>().HasData(tripList);
        }

        private static List<Trip> GetInitialTrips()
        {
            return
            [
                new Trip {Id=Guid.NewGuid(), City="Москва", Country="Россия", DateStart=new DateTime(2025,09,10), DateEnd=new DateTime(2025,10,10), DateCreated=new DateTime(2025,12,30), Description="Пить пиво", Photo="AgACAgIAAxkBAAMmZ21Dg1p1YjFwerlw18vT4y3447MAAjntMRsPbWhLEqJUMCpbLOcBAAMCAANtAAM2BA", Status=TripStatus.New, UserId=737061580},
                new Trip {Id=Guid.NewGuid(), City="Москва", Country="Россия", DateStart=new DateTime(2025,05,05), DateEnd=new DateTime(2025,05,05), DateCreated=new DateTime(2025,12,30), Description="Есть шашлыки", Photo="AgACAgIAAxkBAAMlZ21DT9bXnZXP9WlB9Zpd-EWOJZAAAjjtMRsPbWhLTRDbFfV5nyEBAAMCAANtAAM2BA", Status=TripStatus.New, UserId=737061580},
                new Trip {Id=Guid.NewGuid(), City="Париж", Country="Франция", DateStart=new DateTime(2026,04,03), DateEnd=new DateTime(2027,04,04), DateCreated=new DateTime(2025,12,30), Description="Целоваться с француженками", Photo="AgACAgIAAxkBAAMkZ21DAufsgeSAXJFq5KO19-iFNvkAAjftMRsPbWhLRTYqd-4NbdIBAAMCAAN4AAM2BA",Status=TripStatus.Declined, UserId=737061580},

                new Trip {Id=Guid.NewGuid(), City="Сеул", Country="Корея", DateStart=new DateTime(2025,03,11), DateEnd=new DateTime(2025,04,21), DateCreated=new DateTime(2025,12,30), Description="Без", Photo="AgACAgIAAxkBAAPMZ21TQsP3P5zUWYbIQyTacmCXfg4AAkDlMRsPbXBLK6C8uUEAAWmbAQADAgADeAADNgQ", Status=TripStatus.New, UserId=6115093206},
                new Trip {Id=Guid.NewGuid(), City="Москва", Country="Россия", DateStart=new DateTime(2024,12,12), DateEnd=new DateTime(2026,12,12), DateCreated=new DateTime(2025,12,30), Description="Бубу бебе", Photo="AgACAgIAAxkBAAPPZ21TbSjQ5QoYV8XRBWYD_Li5irUAAkHlMRsPbXBL_DTPNQTSQdUBAAMCAAN5AAM2BA", Status=TripStatus.Accepted, UserId=6115093206},
                new Trip {Id=Guid.NewGuid(), City="Казань", Country="Россия", DateStart=new DateTime(2025,12,28), DateEnd=new DateTime(2026,12,31), DateCreated=new DateTime(2025,12,30), Description="Беееее", Photo="AgACAgIAAxkBAAPQZ21Th4uzIB-yegS1FS7hivKJm3AAAknlMRsPbXBLz_E9JarqRaYBAAMCAAN4AAM2BA", Status=TripStatus.Declined, UserId=6115093206},
                new Trip {Id=Guid.NewGuid(), City="Казань", Country="Россия", DateStart=new DateTime(2025,12,28), DateEnd=new DateTime(2026,12,31), DateCreated=new DateTime(2025,12,30), Description="Беееее", Photo="AgACAgIAAxkBAAPQZ21Th4uzIB-yegS1FS7hivKJm3AAAknlMRsPbXBLz_E9JarqRaYBAAMCAAN4AAM2BA", Status=TripStatus.OnTheWay, UserId=6115093206},
                new Trip {Id=Guid.NewGuid(), City="Сеул", Country="Корея", DateStart=new DateTime(2025,03,11), DateEnd=new DateTime(2025,04,21), DateCreated=new DateTime(2025,12,30), Description="ЗАМУЖ БЛЯ", Photo="AgACAgIAAxkBAAPSZ21Tya43IshAr3u85ebTgUlD600AAkvlMRsPbXBLFiUYNtX-VuABAAMCAAN5AAM2BA", Status=TripStatus.Ended, UserId=6115093206}
            ];
        }

        private static List<User> GetInitialUsers()
        {
            return
            [
                new User { Id = 737061580, NickName = "abit_z7", Type = UserType.Admin },
                new User { Id = 6115093206, NickName = "murmur303", Type = UserType.SimpleUser }
            ];
        }
    }
}
