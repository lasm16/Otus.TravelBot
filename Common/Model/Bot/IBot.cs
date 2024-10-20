namespace Common.Model.Bot
{
    public interface IBot
    {
        IList<Post> Posts { get; }
        public IList<string> AvailableActions { get; }
        public string SendGreetingMessage();
        public void PerfomAction(string action);
    }
}
