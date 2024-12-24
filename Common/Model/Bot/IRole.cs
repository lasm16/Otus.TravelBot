namespace Common.Model.Bot
{
    public interface IRole
    {
        public Dictionary<string, IScenario>? Actions { get; set; }
    }
}
