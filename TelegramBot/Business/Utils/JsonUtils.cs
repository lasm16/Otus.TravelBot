using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace TelegramBot.Business.Utils
{
    public static class JsonUtils
    {
        public static JsonSerializerOptions GetSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
        }
    }
}
