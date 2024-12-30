namespace Common.Model.Bot
{
    public interface IRole
    {
        public IReadOnlyDictionary<string, IScenario>? Actions { get; set; }
    }
}
