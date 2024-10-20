namespace Common.Model.Bot
{
    public interface IBot
    {
        public IList<string> AvailableActions { get; }
        public string SendGreetingMessage();
        public void PerfomAction(string action);
    }
}
