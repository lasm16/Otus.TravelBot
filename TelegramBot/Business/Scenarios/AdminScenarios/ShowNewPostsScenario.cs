using Common.Model.Bot;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Serilog;
using Telegram.Bot.Polling;
using Common.Model;
using Common.Data;
using TelegramBot.Business.Bot;

namespace TelegramBot.Business.Scenarios.AdminScenarios
{
    public class ShowNewPostsScenario(TelegramBotClient botClient) : IScenario
    {
        private static List<Post> _posts = Repository.Posts;
        private TelegramBotClient _botClient = botClient;

        public void Launch()
        {
            SubscribeEvents();
        }

        private async Task OnUpdate(Update update)
        {
            if (update.CallbackQuery.Data.Equals("Новые посты"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var newTrips = GetTrips();
                if (newTrips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.PostsNotFound);
                    return;
                }
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Посмотреть", "Принять все", "Отклонить все");
                await _botClient.SendMessage(chatId, BotPhrases.PostsFound + $" ({newTrips.Count}):", replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Принять все"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                //var newPostsLits = _posts.Select(x => x.Trips.Where(c => c.Status == TripStatus.New).ToList()).ToList();
                //foreach (var list in newPostsLits)
                //{

                //}
                //var list1 = newPostsLits.se
                //var newTripsList = _posts.Where(x => x.Status == PostStatus.New).ToList();
                //foreach (var trip in newTripsList)
                //{
                //    trip.Status = PostStatus.Accepted;
                //}
            }
            if (update.CallbackQuery.Data.Equals("Отклоноить все"))
            { }
            if (update.CallbackQuery.Data.Equals("Посмотреть"))
            { }
        }

        private static List<Trip> GetTrips()
        {
            var ss = _posts.Select(x => x.Trips.Where(c => c.Status == TripStatus.New).ToList()).ToList();
            return null;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            throw new NotImplementedException();
        }

        private void UnsubscribeEvents()
        {
            _botClient.OnError -= OnError;
            _botClient.OnMessage -= OnMessage;
            _botClient.OnUpdate -= OnUpdate;
        }

        private void SubscribeEvents()
        {
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }
    }
}
