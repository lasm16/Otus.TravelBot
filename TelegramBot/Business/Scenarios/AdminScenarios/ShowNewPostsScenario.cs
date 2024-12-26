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
using Telegram.Bot.Exceptions;

namespace TelegramBot.Business.Scenarios.AdminScenarios
{
    public class ShowNewPostsScenario(TelegramBotClient botClient) : IScenario
    {
        private object? _currentTrip;
        private int _confirmMessageId = 0;
        private int _messageIdForPostsCount = 0;
        private TelegramBotClient _botClient = botClient;
        private static List<Post> _posts = Repository.Posts;
        private List<object> _trips = GetNewTripsWithUserName();

        private readonly string _launchCommand = AppConfig.LaunchCommand;

        public void Launch()
        {
            SubscribeEvents();
        }

        private async Task OnUpdate(Update update)
        {
            var button = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery!.Message!.Chat.Id;
            var messageId = update.CallbackQuery.Message.Id;
            await ButtonClick(button, chatId, messageId);
        }

        private async Task ButtonClick(string? button, long chatId, int messageId)
        {
            switch (button)
            {
                case "Новые посты":
                    await NewPostsClick(chatId);
                    break;
                case "Далее":
                    await NextClick(chatId, messageId);
                    break;
                case "Назад":
                    await PreviousClick(chatId, messageId);
                    break;
                case "Посмотреть":
                    await ShowClick(chatId, messageId);
                    break;
                case "Принять все":
                    await AcceptAllClick(chatId, messageId);
                    break;
                case "Отклонить все":
                    await DeclineAllClick(chatId, messageId);
                    break;
                case "Принять":
                    await AcceptClick(chatId, messageId);
                    break;
                case "Отклонить":
                    await DeclineClick(chatId, messageId);
                    break;
                default:
                    return;
            }
        }

        private async Task DeclineClick(long chatId, int messageId)
        {
            var tripToDecline = _currentTrip;
            //Сохранение данных в БД
            var index = _trips.IndexOf(tripToDecline);
            _trips.Remove(tripToDecline); // Заменить на реальное сохранение
            if (_trips.Count == 0)
            {
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_trips.Count}):");
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
            await _botClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.PostsFound + $" ({_trips.Count}):");

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
        }

        private async Task AcceptClick(long chatId, int messageId)
        {
            var tripToAccept = _currentTrip;
            //Сохранение данных в БД
            var index = _trips.IndexOf(tripToAccept);
            _trips.Remove(tripToAccept);// Заменить на реальное сохранение
            _trips.Remove(tripToAccept);// Заменить на реальное сохранение
            if (_trips.Count == 0)
            {
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_trips.Count}):");
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
            await _botClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.PostsFound + $" ({_trips.Count}):");

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
        }

        private async Task DeclineAllClick(long chatId, int messageId)
        {
            var newTrips = GetNewTrips();
            newTrips.ForEach(x => x.Status = TripStatus.Declined);
            //сохранить в БД
            await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            await _botClient.SendMessage(chatId, BotPhrases.AllTripsDeclined);
        }

        private async Task AcceptAllClick(long chatId, int messageId)
        {
            var newTrips = GetNewTrips();
            newTrips.ForEach(x => x.Status = TripStatus.Accepted);
            //сохранить в БД
            await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            await _botClient.SendMessage(chatId, BotPhrases.AllTripsAccepted);
        }

        private async Task ShowClick(long chatId, int messageId)
        {
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
            var botMessage = await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var index = _trips.IndexOf(_currentTrip) - 1;
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task NextClick(long chatId, int messageId)
        {
            var index = _trips.IndexOf(_currentTrip) + 1;
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
            if (index == _trips.Count - 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task NewPostsClick(long chatId)
        {
            var newTrips = GetNewTrips();
            if (newTrips.Count == 0)
            {
                await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Посмотреть", "Принять все", "Отклонить все");
            var botMessage = await _botClient.SendMessage(chatId, BotPhrases.PostsFound + $" ({newTrips.Count}):", replyMarkup: inlineMarkup);
            _messageIdForPostsCount = botMessage.MessageId;
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
                var trips = post.Trips.Where(x => x.Status == TripStatus.New).ToArray();
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
            Console.WriteLine(exception.Message, exception.StackTrace);
            Log.Debug(exception.Message, exception.StackTrace);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var inputLine = message.Text;
            var chatId = message.Chat.Id;
            if (inputLine is null)
            {
                return;
            }
            if (!inputLine.Equals(_launchCommand))
            {
                Log.Error("Некорректно указан сценарий!");
                await _botClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
                return;
            }
            if (_confirmMessageId != 0)
            {
                try
                {
                    await _botClient.EditMessageReplyMarkup(chatId, _confirmMessageId, null);
                }
                catch (ApiRequestException ex)
                {
                    Log.Error(ex.Message, ex.StackTrace);
                    Console.WriteLine(ex.Message, ex.StackTrace);
                }
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
