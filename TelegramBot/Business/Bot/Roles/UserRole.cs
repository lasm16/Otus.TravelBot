using Common.Model.Bot;

namespace TelegramBot.Business.Bot.Roles
{
    public class UserRole() : IBotRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
