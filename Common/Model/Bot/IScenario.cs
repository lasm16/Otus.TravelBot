namespace Common.Model.Bot
{
    public interface IScenario
    {
        string Text { get; set; }
        void DoAction();
    }
}
