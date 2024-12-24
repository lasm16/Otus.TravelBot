using Common.Model.Bot;

namespace TelegramBot.Business.Bot.Roles
{
    public class UserRole() : IRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
