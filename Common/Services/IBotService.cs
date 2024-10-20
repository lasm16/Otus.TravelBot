namespace Common.Services
{
    public interface IBotService
    {
        public void Greeting();
        public IList<string> Actions { get; }
        public void LaunchScenario(string scenarioName);
    }
}
