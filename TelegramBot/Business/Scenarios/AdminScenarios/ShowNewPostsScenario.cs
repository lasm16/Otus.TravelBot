using Common.Model.Bot;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Serilog;
using Telegram.Bot.Polling;
using Common.Model;
using Common.Data;
using TelegramBot.Business.Bot;
using System.Text;

namespace TelegramBot.Business.Scenarios.AdminScenarios
{
    public class ShowNewPostsScenario(TelegramBotClient botClient) : IScenario
    {
        private object? _currentTrip;
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
                var newTrips = GetNewTrips();
                if (newTrips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Посмотреть", "Принять все", "Отклонить все");
                await _botClient.SendMessage(chatId, BotPhrases.PostsFound + $" ({newTrips.Count}):", replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Принять все"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var newTrips = GetNewTrips();
                newTrips.ForEach(x => x.Status = TripStatus.Accepted);
                //сохранить в БД
                await _botClient.SendMessage(chatId, BotPhrases.AllTripsAccepted);
            }
            if (update.CallbackQuery.Data.Equals("Отклоноить все"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var newTrips = GetNewTrips();
                newTrips.ForEach(x => x.Status = TripStatus.Declined);
                //сохранить в БД
                await _botClient.SendMessage(chatId, BotPhrases.AllTripsDeclined);
            }
            if (update.CallbackQuery.Data.Equals("Посмотреть"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                var trips = GetNewTripsWithUserName();
                if (trips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                await _botClient.DeleteMessage(chatId, messageId);
                var tripObject = trips[0];
                _currentTrip = tripObject;
                var (text, photo) = GetTripText(tripObject);

                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить");

                if (trips.Count > 1)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
                }
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
        }

        private static (string text, string photo) GetTripText(object tripObject)
        {
            var trip = new { City = "", DateStart = "", DateEnd = "", Description = "", Photo = "", UserName = "" };
            trip = CastToAnonymousType(trip, tripObject);

            var status = TelegramBotImpl.GetStatus(TripStatus.New);
            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + trip.UserName);

            var text = message.ToString();
            var photo = trip.Photo;
            return (text, photo);
        }

        private static List<Trip> GetNewTrips() =>
            _posts
                .SelectMany(c => c.Trips
                .Where(x => x.Status == TripStatus.New))
                .ToList();


        private static List<object> GetNewTripsWithUserName()
        {
            // не нравятся двойные массивы
            List<object> list = [];
            foreach (var post in _posts)
            {
                var userName = post.User.UserName;
                var trips = post.Trips.Where(x => x.Status == TripStatus.New).ToList();
                foreach (var trip in trips)
                {
                    var userWithTrips = new
                    {
                        trip.City,
                        trip.DateStart,
                        trip.DateEnd,
                        trip.Description,
                        trip.Photo,
                        UserName = userName,
                    };
                    list.Add(userWithTrips);
                }
            }
            return list;
        }

        private static T CastToAnonymousType<T>(T typeHolder, Object x) => (T)x;

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
