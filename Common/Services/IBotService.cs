namespace Common.Services
{
    /// <summary>
    /// Сервис, запусакающий нужный сценарий работы бота
    /// </summary>
    public interface IBotService
    {
        public void Greeting();
        public IList<string> Actions { get; }
        public void LaunchScenario(string scenarioName);
    }
}
