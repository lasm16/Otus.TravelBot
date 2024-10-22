namespace Common.Model.Bot
{
    public interface IActionStrategy
    {
        /// <summary>
        /// Команда, которую будет выполнять бот
        /// </summary>
        public void DoAction();
    }
}
