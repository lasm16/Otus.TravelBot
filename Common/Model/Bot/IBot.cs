namespace Common.Model.Bot
{
    /// <summary>
    /// Бот, который будет выполнять команды
    /// </summary>
    public interface IBot
    {
        public IList<string> AvailableActions { get; }
        public string SendGreetingMessage();
        public void PerfomAction(string action);
    }
}
