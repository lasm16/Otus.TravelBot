using Common.Model.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Business.Scenarios
{
    public class GreetingsScenario(Message message) : IScenario
    {
        private Message _message = message;

        public string? Text { get; set; }
        public InlineKeyboardMarkup InlineKeyboard { get; set; }

        public void DoAction()
        {
            var currentUser = _message.From!.FirstName + " " + _message.From.LastName;
            Text = $"Приветствую тебя, {currentUser}! Ты можешь выложить пост о планируемой поездке или найти попутчика.";


            var inlineMarkup = new InlineKeyboardMarkup()
                .AddButton("Новая поездка", "CreateNewTripScenario")
                .AddButton("Найти попутчика", "FindFellowScenario");

            InlineKeyboard = inlineMarkup;
        }
    }
}
