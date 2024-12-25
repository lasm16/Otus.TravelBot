using Common.Data;
using Common.Model;
using Common.Model.Bot;
using Serilog;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Business.Bot;

namespace TelegramBot.Business.Scenarios.UserScenarios
{
    public class FindFellowScenario : IScenario
    {
        private object? _currentTrip;
        private Common.Model.User _user;
        private List<object> _trips = [];
        private List<Post> _afterFilterPosts = [];
        private readonly TelegramBotClient _botClient;
        private static List<Post> _posts = Repository.Posts;

        public FindFellowScenario(TelegramBotClient botClient, Common.Model.User user)
        {
            _user = user;
            _botClient = botClient;
            _afterFilterPosts = FilterPosts();
        }

        public void Launch()
        {
            SubscribeEvents();
        }

        private async Task OnUpdate(Update update)
        {
            var button = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery!.Message!.Chat.Id;
            var messageId = update.CallbackQuery.Message.Id;
            if (_posts == null)
            {
                await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            await ButtonClick(button, chatId, messageId);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var inputLine = message.Text;
            if (inputLine is null)
            {
                return;
            }
            if (inputLine.Equals("/start"))
            {
                UnsubscribeEvents();
                var scenario = new GreetingScenario(_botClient);
                scenario.Launch();
                return;
            }
            var chatId = message.Chat.Id;
            await SearchTripsWithInputLine(inputLine, chatId);
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task SearchTripsWithInputLine(string inputLine, long chatId)
        {
            _trips = GetTrips(inputLine);

            if (_trips.Count == 0)
            {
                await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            var trip = _trips[0];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);

            InlineKeyboardMarkup inlineMarkup = null;

            if (_trips.Count > 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
            }
            await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
        }

        private static (string text, string photo) GetTripText(object tripObject)
        {
            var trip = new { City = "", DateStart = "", DateEnd = "", Description = "", Photo = "", Status = TripStatus.Declined, UserName = "" };
            trip = CastToAnonymousType(trip, tripObject);

            var status = TelegramBotImpl.GetStatus(trip.Status);
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

        private static T CastToAnonymousType<T>(T typeHolder, Object x) => (T)x;

        private List<object> GetTrips(string inputLine)
        {
            // не нравятся двойные массивы
            List<object> list = [];
            foreach (var post in _afterFilterPosts)
            {
                var userName = post.User.UserName;
                var isDate = DateTime.TryParse(inputLine, out var date);
                List<Trip> trips = [];
                if (isDate)
                {
                    trips = post.Trips.Where(x => x.DateStart.Equals(inputLine)).ToList();
                }
                else
                {
                    trips = post.Trips.Where(x => x.City.Equals(inputLine)).ToList();
                }
                foreach (var trip in trips)
                {
                    var userWithTrips = new
                    {
                        trip.City,
                        trip.DateStart,
                        trip.DateEnd,
                        trip.Description,
                        trip.Photo,
                        trip.Status,
                        UserName = userName,
                    };
                    list.Add(userWithTrips);
                }
            }
            return list;
        }

        private List<Post> FilterPosts()
        {
            var list = new List<Post>();
            var userName = _user.UserName;
            var posts = GetPostsWithoutCurrentUser(userName);
            foreach (var post in posts)
            {
                var trips = post.Trips.Where(x => x.Status == TripStatus.Accepted || x.Status == TripStatus.OnTheWay).ToList();
                var newPost = new Post()
                {
                    User = post.User,
                    Trips = trips
                };
                list.Add(newPost);
            }
            return list;
        }

        private static List<Post> GetPostsWithoutCurrentUser(string userName) =>
            _posts
                .Where(x => !x.User.UserName.Equals(userName))
                .ToList();

        private void SubscribeEvents()
        {
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }

        private void UnsubscribeEvents()
        {
            _botClient.OnError -= OnError;
            _botClient.OnMessage -= OnMessage;
            _botClient.OnUpdate -= OnUpdate;
        }

        private async Task ButtonClick(string? button, long chatId, int messageId)
        {
            switch (button)
            {
                case "Далее":
                    await NextClick(chatId, messageId);
                    break;
                case "Назад":
                    await PreviousClick(chatId, messageId);
                    break;
                case "Найти попутчика":
                    await FindFellowClick(chatId, messageId, BotPhrases.SearchType);
                    break;
                default:
                    return;
            }
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var index = _trips.IndexOf(_currentTrip) - 1;
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
            }
            await _botClient.DeleteMessage(chatId, messageId);
            await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
        }

        private async Task NextClick(long chatId, int messageId)
        {
            var index = _trips.IndexOf(_currentTrip) + 1;
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Назад", "Далее");
            if (index == _trips.Count - 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Назад");
            }
            await _botClient.DeleteMessage(chatId, messageId);
            await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
        }

        private async Task FindFellowClick(long chatId, int messageId, string message)
        {
            await _botClient.SendMessage(chatId, message);
            await _botClient.EditMessageReplyMarkup(chatId, messageId);
        }
    }
}
