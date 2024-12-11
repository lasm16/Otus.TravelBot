using Telegram.Bot.Types.ReplyMarkups;

namespace Common.Model.Bot
{
    public interface IScenario
    {
        public InlineKeyboardMarkup InlineKeyboard { get; set; }
        string? Text { get; set; }
        void DoAction();
    }
}
