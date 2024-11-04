using Common.Model.Bot;

namespace Common.Services
{
    /// <summary>
    /// Сервис, запусакающий нужный сценарий работы бота
    /// </summary>
    public interface IBotService
    {
        public void LaunchScenario(IAction action);
    }
}
