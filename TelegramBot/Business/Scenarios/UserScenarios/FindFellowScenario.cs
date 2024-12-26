using Common.Data;
using Common.Model;
using Common.Model.Bot;
using Serilog;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
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
        private int _currentMessageId = 0;
        private List<object> _searchedTrips = [];
        private List<Post> _postsWithFilter = [];
        private readonly TelegramBotClient _botClient;
        private static List<Post> _posts = Repository.Posts;

        private readonly string _launchCommand = AppConfig.LaunchCommand;

        public FindFellowScenario(TelegramBotClient botClient, Common.Model.User user)
        {
            _user = user;
            _botClient = botClient;
            _postsWithFilter = FilterPosts();
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
            var chatId = message.Chat.Id;
            if (inputLine is null)
            {
                return;
            }
            if (inputLine.Equals(_launchCommand))
            {
                if (_currentMessageId != 0)
                {
                    try
                    {
                        await _botClient.EditMessageReplyMarkup(chatId, _currentMessageId, null);
                    }
                    catch (ApiRequestException e)
                    {
                        Log.Error(e.Message, e.StackTrace);
                    }
                }
                UnsubscribeEvents();
                var scenario = new GreetingScenario(_botClient);
                scenario.Launch();
                return;
            }
            if (_currentMessageId != 0)
            {
                await _botClient.EditMessageReplyMarkup(chatId, _currentMessageId, null);
                _currentMessageId = 0;
            }
            await SearchTripsWithInputLine(inputLine, chatId);
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task SearchTripsWithInputLine(string inputLine, long chatId)
        {
            _searchedTrips = GetTrips(inputLine);

            if (_searchedTrips.Count == 0)
            {
                await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            var trip = _searchedTrips[0];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);

            InlineKeyboardMarkup inlineMarkup = null;

            if (_searchedTrips.Count > 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
            }
            var message = BotPhrases.TripsFound + $" ({_searchedTrips.Count}):";
            await _botClient.SendMessage(chatId, message);

            try
            {
                await _botClient.DeleteMessage(chatId, _currentMessageId);
            }
            catch (ApiRequestException e)
            {
                Log.Error(e.Message, e.StackTrace);
                Console.WriteLine(e.Message, e.StackTrace);
            }
            var botMessage = _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.Result.MessageId;
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
            foreach (var post in _postsWithFilter)
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

        private List<object> GetTrips()
        {
            // не нравятся двойные массивы
            List<object> list = [];
            foreach (var post in _postsWithFilter)
            {
                var userName = post.User.UserName;
                var trips = post.Trips;
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
            foreach (var post in _posts)
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
                case "Показать все":
                    await ShowAllClick(chatId, messageId);
                    break;
                default:
                    return;
            }
        }

        private async Task ShowAllClick(long chatId, int messageId)
        {
            _searchedTrips = GetTrips();
            if (_searchedTrips.Count == 0)
            {
                await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }

            var trip = _searchedTrips[0];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);

            InlineKeyboardMarkup inlineMarkup = null;

            if (_searchedTrips.Count > 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
            }

            await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            var message = BotPhrases.TripsFound + $" ({_searchedTrips.Count}):";
            await _botClient.SendMessage(chatId, message);
            var botMessage = await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var index = _searchedTrips.IndexOf(_currentTrip) - 1;
            var trip = _searchedTrips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;

        }

        private async Task NextClick(long chatId, int messageId)
        {
            var index = _searchedTrips.IndexOf(_currentTrip) + 1;
            var trip = _searchedTrips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Назад", "Далее");
            if (index == _searchedTrips.Count - 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Назад");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;
        }

        private async Task FindFellowClick(long chatId, int messageId, string message)
        {
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Показать все");
            var botMessage = await _botClient.SendMessage(chatId, message, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;
            await _botClient.EditMessageReplyMarkup(chatId, messageId);
        }
    }
}
