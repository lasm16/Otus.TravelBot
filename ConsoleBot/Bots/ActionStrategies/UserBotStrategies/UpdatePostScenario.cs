using Common.Bot;
using Common.Model;
using Common.Model.Bot;
using ConsoleBot.Data;

namespace ConsoleBot.Bots.ActionStrategies.UserBotStrategies
{
    internal class UpdatePostScenario : IActionStrategy
    {
        public void DoAction(IUser user)
        {
            var inputLine = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.PostForUpdate);
            Guid.TryParse(inputLine, out var guid);

            var dateTimeStartString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestStartDate);
            DateTime.TryParse(dateTimeStartString, out var startDate);

            var dateTimeEndString = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.SuggestEndDate);
            DateTime.TryParse(dateTimeEndString, out var endDate);

            var description = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.Description);
            var link = ConsoleLineExtractor.GetLineFromConsole(BotPhrases.LinkVk);

            UpdatePost(user, guid, startDate, endDate, description, link);
        }

        private void UpdatePost(IUser user, Guid guid, DateTime dateTimeStart, DateTime dateTimeEnd, string description, string link)
        {
            var post = user.Posts.FirstOrDefault(x => x.Id == guid);

            if (post != null)
            {
                post.TravelDateStart = dateTimeStart;
                post.TravelDateEnd = dateTimeEnd;
                post.Description = description;
                post.LinkToVk = link;
                post.Status = "Проверяется";
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }
    }
}