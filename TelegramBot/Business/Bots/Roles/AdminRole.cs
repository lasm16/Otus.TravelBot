using Common.Model.Bot;

namespace TelegramBot.Business.Bots.Roles
{
    internal class AdminRole() : IBotRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
