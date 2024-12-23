using Common.Model.Bot;

namespace TelegramBot.Business.Bots.Roles
{
    public class UserRole() : IBotRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
