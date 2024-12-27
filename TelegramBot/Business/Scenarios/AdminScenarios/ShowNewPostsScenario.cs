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
    public class ShowNewPostsScenario(TelegramBotClient botClient) : BaseScenario(botClient), IScenario
    {
        private Trip? _currentTrip;
        private int _confirmMessageId = 0;
        private int _messageIdForPostsCount = 0;
        private static List<Post> _posts = Repository.Posts;
        private List<Trip> _trips = [];
        private Dictionary<string, List<Trip>> _userWithTrips = GetNewTripsWithUserNameDict();

        private readonly string _launchCommand = AppConfig.LaunchCommand;

        public void Launch()
        {
            SubscribeEvents();
            _trips = GetTripListFromNewPosts();
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
                await BotClient.DeleteMessage(chatId, messageId);
                await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_trips.Count}):");
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            if (_trips.Count == index)
            {
                index--;
            }
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripTextWithPhoto(trip);
            var inlineMarkup = GetNavigationButtons(_trips.Count, index);
            await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.PostsFound + $" ({_trips.Count}):");

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
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
                await BotClient.DeleteMessage(chatId, messageId);
                await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_trips.Count}):");
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            if (_trips.Count == index)
            {
                index--;
            }
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripTextWithPhoto(trip);
            var inlineMarkup = GetNavigationButtons(_trips.Count, index);
            await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.PostsFound + $" ({_trips.Count}):");

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
        }

        private async Task DeclineAllClick(long chatId, int messageId)
        {
            var list = GetTripListFromNewPosts();
            //сохранить в БД
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            await BotClient.SendMessage(chatId, BotPhrases.AllTripsDeclined);
        }

        private async Task AcceptAllClick(long chatId, int messageId)
        {
            var list = GetTripListFromNewPosts();
            //сохранить в БД
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            await BotClient.SendMessage(chatId, BotPhrases.AllTripsAccepted);
        }

        private List<Trip> GetTripListFromNewPosts()
        {
            var tripsFromMap = _userWithTrips.Values;
            var list = new List<Trip>();
            foreach (var trips in tripsFromMap)
            {
                var newTrip = new Trip();
                foreach (var trip in trips)
                {
                    newTrip = trip;
                    newTrip.Status = TripStatus.Accepted;
                    list.Add(newTrip);
                }
            }
            return list;
        }

        private async Task ShowClick(long chatId, int messageId)
        {
            var newTripsCount = _userWithTrips.Values.Sum(list => list.Count);
            if (newTripsCount == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            var keyValue = _userWithTrips.FirstOrDefault();
            var userName = keyValue.Key;
            var trip = keyValue.Value.FirstOrDefault();
            _currentTrip = trip;
            var (text, photo) = GetTripTextWithPhoto(trip);

            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить");

            if (_trips.Count > 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
            }
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            var botMessage = await BotClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var index = _trips.IndexOf(_currentTrip) - 1;
            var trip = _trips[index];
            _currentTrip = trip;
            string userName = null;
            var (text, photo) = GetTripTextWithPhoto(trip);
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task NextClick(long chatId, int messageId)
        {
            var index = _trips.IndexOf(_currentTrip) + 1;
            var trip = _trips[index];
            _currentTrip = trip;
            var (text, photo) = GetTripTextWithPhoto(trip);
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
            if (index == _trips.Count - 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private string GetUserName(Trip trip)
        {
            string userName = null;
            foreach (var userNameInDic in from pair in _userWithTrips
                                          let userNameInDic = pair.Key
                                          let trips = pair.Value
                                          from item in trips
                                          where item == trip
                                          select userNameInDic)
            {
                userName = userNameInDic;
            }

            return userName;
        }

        private async Task NewPostsClick(long chatId)
        {
            var newTripsCount = _userWithTrips.Values.Sum(list => list.Count);
            if (newTripsCount == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Посмотреть", "Принять все", "Отклонить все");
            var botMessage = await BotClient.SendMessage(chatId, BotPhrases.PostsFound + $" ({newTripsCount}):", replyMarkup: inlineMarkup);
            _messageIdForPostsCount = botMessage.MessageId;
        }

        private static InlineKeyboardMarkup? GetNavigationButtons(int count, int index)
        {
            InlineKeyboardMarkup? inlineMarkup = null;
            if (count == 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить");
            }
            if (count > 1 && index == 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
            }
            if (count > 1 && index != 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад", "Далее");
            }
            if (count > 1 && index == count - 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Назад");
            }
            return inlineMarkup;
        }

        private (string text, string photo) GetTripTextWithPhoto(Trip trip)
        {
            var userName = GetUserName(trip);
            var status = Helper.GetStatus(TripStatus.New);

            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + userName);

            var text = message.ToString();
            var photo = trip.Photo;
            return (text, photo);
        }

        private static Dictionary<string, List<Trip>> GetNewTripsWithUserNameDict()
        {
            var map = new Dictionary<string, List<Trip>>();
            foreach (var post in _posts)
            {
                var userName = post.User.UserName;
                var trips = post.Trips.Where(x => x.Status == TripStatus.New).ToList();
                map.Add(userName, trips);
            }
            return map;
        }

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
                await BotClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
                return;
            }
            if (_confirmMessageId != 0)
            {
                try
                {
                    await BotClient.EditMessageReplyMarkup(chatId, _confirmMessageId, null);
                }
                catch (ApiRequestException ex)
                {
                    Log.Error(ex.Message, ex.StackTrace);
                    Console.WriteLine(ex.Message, ex.StackTrace);
                }
            }
            UnsubscribeEvents();
            var scenario = new GreetingScenario(BotClient);
            scenario.Launch();
        }

        private void UnsubscribeEvents()
        {
            BotClient.OnError -= OnError;
            BotClient.OnMessage -= OnMessage;
            BotClient.OnUpdate -= OnUpdate;
        }

        private void SubscribeEvents()
        {
            BotClient.OnError += OnError;
            BotClient.OnMessage += OnMessage;
            BotClient.OnUpdate += OnUpdate;
        }
    }
}
