using Common.Model.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Business.Scenarios
{
    public class GreetingsScenario(Message message) : IScenario
    {
        private Message _message = message;

        public string? Text { get; set; }

        public void DoAction()
        {
            var currentUser = _message.From!.FirstName + " " + _message.From.LastName;
            Text = $"Приветствую тебя, {currentUser}! Ты можешь выложить пост о планируемой поездке или найти попутчика";
        }
    }
}
