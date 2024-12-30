using Common.Model.Bot;

namespace TelegramBot.Business.Bot.Roles
{
    internal class AdminRole() : IRole
    {
        public IReadOnlyDictionary<string, IScenario>? Actions { get; set; }
    }
}
