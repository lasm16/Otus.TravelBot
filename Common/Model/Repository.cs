using System.Text.Json;

namespace Common.Model
{
    /// <summary>
    /// Потому удалю, когда будет БД
    /// </summary>
    public class Repository
    {
        public static List<Trip>? Trips => GetFromFile();

        private static List<Trip>? GetFromFile()
        {
            var fs = new FileStream("trips.json", FileMode.OpenOrCreate);
            return JsonSerializer.Deserialize<List<Trip>>(fs);
        }
    }
}
