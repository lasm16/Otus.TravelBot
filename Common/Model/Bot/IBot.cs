namespace Common.Model.Bot
{
    /// <summary>
    /// Бот, который будет выполнять команды
    /// </summary>
    public interface IBot
    {
        public Dictionary<string, IAction> Actions { get; }
        public string GreetingMessage { get; }
    }
}
