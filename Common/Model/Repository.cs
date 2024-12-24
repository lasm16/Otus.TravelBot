using System.Text.Json;

namespace Common.Model
{
    /// <summary>
    /// Потому удалю, когда будет БД
    /// </summary>
    public class Repository
    {
        public static List<Post>? Posts => GetFromFile();

        private static List<Post>? GetFromFile()
        {
            using var fs = new FileStream("posts.json", FileMode.OpenOrCreate);
            return JsonSerializer.Deserialize<List<Post>>(fs);
        }
    }
}
