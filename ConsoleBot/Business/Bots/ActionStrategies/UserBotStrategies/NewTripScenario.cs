using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Business.Bots.ActionStrategies.UserBotStrategies
{
    internal class NewTripScenario(User user) : IActionStrategy
    {
        public void DoAction()
        {
            Console.WriteLine(BotPhrases.Agreement);

            var dateTimeStartString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestStartDate);
            DateTime.TryParse(dateTimeStartString, out var startDate);

            var dateTimeEndString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestEndDate);
            DateTime.TryParse(dateTimeEndString, out var endDate);

            var description = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.Description);
            var link = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.LinkVk);

            CreateNewPost(user, startDate, endDate, description, link);
            Console.WriteLine(BotPhrases.Done);
        }

        private void CreateNewPost(User user, DateTime dateTimeStart, DateTime dateTimeEnd, string description, string link)
        {
            var post = new Post(new Guid(), dateTimeStart, dateTimeEnd, description, link);
            user.Posts.Add(post);
        }
    }
}
