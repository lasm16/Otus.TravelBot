using Common.Model.Bot;

namespace TelegramBot.Business.Bot.Roles
{
    internal class AdminRole() : IRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
