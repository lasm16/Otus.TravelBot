namespace Common.Model.Bot
{
    /// <summary>
    /// Бот, который будет выполнять команды
    /// </summary>
    public interface IBot
    {
        public List<IAction> Actions { get; }
        public string GreetingMessage { get; }
    }
}
