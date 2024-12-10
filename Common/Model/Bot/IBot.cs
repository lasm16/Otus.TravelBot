namespace Common.Model.Bot
{
    /// <summary>
    /// Бот для различных реализаций
    /// </summary>
    public interface IBot
    {
        public Task CreateBotAsync();
    }
}
