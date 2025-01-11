using DataBase.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Business.Bot
{
    public class Helper()
    {
        public static InlineKeyboardMarkup GetInlineKeyboardMarkup(params string[] buttonsName)
        {
            var inlineMarkup = new InlineKeyboardMarkup();
            foreach (var button in buttonsName)
            {
                inlineMarkup.AddButton(button, button);
            }
            return inlineMarkup;
        }

        public static string GetStatus(TripStatus status)
        {
            return status switch
            {
                TripStatus.New => "на рассмотрении администрацией.",
                TripStatus.Accepted => "запланирована.",
                TripStatus.Declined => "отклонена администрацией. Удалите пост и создайте новый.",
                TripStatus.OnTheWay => "в пути.",
                TripStatus.Ended => "завершена.",
                _ => "Unknown",
            };
        }
    }
}
