namespace Common.Services
{
    /// <summary>
    /// Сервис, запусакающий бота
    /// </summary>
    public interface IBotService
    {
        public Task StartAsync();
    }
}
