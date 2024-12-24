using Common.Model.Bot;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Serilog;
using Telegram.Bot.Polling;
using Common.Model;

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
                var newPostList = _posts.Where(x => x.Trips.Any()).ToList();
                //var newTripsList = _posts.Select(x => x.Trips.Where(y => y.Status == TripStatus.New).ToList()).ToList();
                //var newTripList = newPostList.Select(x => x.Trips.Where(v=>v.Status==TripStatus.New).ToList());
                //if (newTripsList.Count == 0)
                //{
                //    await _botClient.SendMessage(chatId, BotPhrases.PostsNotFound);
                //    return;
                //}
                //var inlineMarkup = new InlineKeyboardMarkup()
                //    .AddButton("Посмотреть", "Посмотреть")
                //    .AddButton("Принять все", "Принять все");
                //await _botClient.SendMessage(chatId, BotPhrases.PostsFound + $" ({newTripsList.Count}):", replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Принять все"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                //var newTripsList = _posts.Where(x => x.Status == PostStatus.New).ToList();
                //foreach (var trip in newTripsList)
                //{
                //    trip.Status = PostStatus.Accepted;
                //}
            }
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
