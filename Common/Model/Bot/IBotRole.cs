namespace Common.Model.Bot
{
    public interface IBotRole
    {
        public Dictionary<string, IScenario> Actions { get; set; }
    }
}
