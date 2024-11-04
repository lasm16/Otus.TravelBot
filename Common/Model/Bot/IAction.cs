namespace Common.Model.Bot
{
    public interface IAction
    {
        /// <summary>
        /// Команда, которую будет выполнять бот
        /// </summary>
        public void DoAction();
    }
}
