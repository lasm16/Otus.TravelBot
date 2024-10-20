using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Bots.ActionStrategies.UserBotStrategies
{
    internal class FindFellowScenario : IActionStrategy
    {
        public void DoAction(IUser user)
        {
            var dateTime = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestDate);
            DateTime.TryParse(dateTime, out DateTime dateTimeOut);
            SearchFellow(user, dateTimeOut);
            Console.WriteLine(BotPhrases.Done);
        }

        private void SearchFellow(IUser user, DateTime dateTime)
        {
            //_users.Remove(_currentUser);

            //var posts = _users.Select(x => x.Posts.Find(p => p.TravelDateStart == dateTimeOut)).ToList();
            //if (posts == null)
            //{
            //    Console.WriteLine($"Поездок с датой = {dateTime} не найдено!");
            //}
            //return posts;
        }
    }
}