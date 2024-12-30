using Common.Model.Bot;

namespace TelegramBot.Business.Bot.Roles
{
    public class UserRole() : IRole
    {
        public IReadOnlyDictionary<string, IScenario>? Actions { get; set; }
    }
}
