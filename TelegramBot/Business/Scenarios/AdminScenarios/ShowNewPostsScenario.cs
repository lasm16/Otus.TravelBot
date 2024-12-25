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
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Business.Scenarios.AdminScenarios
{
    public class ShowNewPostsScenario(TelegramBotClient botClient) : IScenario
    {
        private object? _currentTrip;
        private List<object> _trips = GetNewTripsWithUserName();
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
                var messageId = update.CallbackQuery.Message.Id;
                var newTrips = GetNewTrips();
                newTrips.ForEach(x => x.Status = TripStatus.Accepted);
                //сохранить в БД
                await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
                await _botClient.SendMessage(chatId, BotPhrases.AllTripsAccepted);
            }
            if (update.CallbackQuery.Data.Equals("Отклонить все"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                var newTrips = GetNewTrips();
                newTrips.ForEach(x => x.Status = TripStatus.Declined);
                //сохранить в БД
                await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
                await _botClient.SendMessage(chatId, BotPhrases.AllTripsDeclined);
            }
            if (update.CallbackQuery.Data.Equals("Посмотреть"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                if (_trips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                var trip = _trips[0];
                _currentTrip = trip;
                var (text, photo) = GetTripText(trip);

                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить");

                if (_trips.Count > 1)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
                }
                await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Далее"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                var index = _trips.IndexOf(_currentTrip) + 1;
                var trip = _trips[index];
                _currentTrip = trip;
                var (text, photo) = GetTripText(trip);
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
                if (index == _trips.Count - 1)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад");
                }
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Назад"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                var index = _trips.IndexOf(_currentTrip) - 1;
                var trip = _trips[index];
                _currentTrip = trip;
                var (text, photo) = GetTripText(trip);
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
                if (index == 0)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
                }
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Принять") || update.CallbackQuery.Data.Equals("Отклонить"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                var tripToAccept = _currentTrip;
                //Сохранение данных в БД
                var index = _trips.IndexOf(tripToAccept);
                _trips.Remove(tripToAccept);
                await _botClient.DeleteMessage(chatId, messageId);
                if (_trips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                if (_trips.Count == index)
                {
                    index--;
                }
                var trip = _trips[index];
                _currentTrip = trip;
                var (text, photo) = GetTripText(trip);
                var inlineMarkup = GetNavigationButtons(_trips.Count, index);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
        }

        private static InlineKeyboardMarkup? GetNavigationButtons(int count, int index)
        {
            InlineKeyboardMarkup? inlineMarkup = null;
            if (count == 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить");
            }
            if (count > 1 && index == 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
            }
            if (count > 1 && index != 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
            }
            if (count > 1 && index == count - 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад");
            }
            return inlineMarkup;
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
            var inputLine = message.Text;
            if (inputLine is null)
            {
                return;
            }
            if (!inputLine.Equals("/start"))
            {
                Log.Error("Некорректно указан сценарий!");
                await _botClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
                return;
            }
            UnsubscribeEvents();
            var scenario = new GreetingScenario(_botClient);
            scenario.Launch();
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
