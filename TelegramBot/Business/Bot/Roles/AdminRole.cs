using Common.Model.Bot;

namespace TelegramBot.Business.Bot.Roles
{
    internal class AdminRole() : IBotRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
