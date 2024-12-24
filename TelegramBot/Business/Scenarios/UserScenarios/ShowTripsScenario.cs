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
    public class ShowTripsScenario(TelegramBotClient botClient, Common.Model.User user) : IScenario
    {
        private Trip? _currentTrip;
        private Post? _post = GetPost(user);
        private static List<Post> _posts = Repository.Posts;
        private readonly TelegramBotClient _botClient = botClient;

        public void Launch()
        {
            SubscribeEvents();
        }

        private async Task OnUpdate(Update update)
        {
            var userName = user.UserName;
            var trips = GetTrips(_post);
            if (update.CallbackQuery.Data.Equals("Мои поездки"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                if (trips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                var trip = trips[0];
                _currentTrip = trip;
                await _botClient.SendMessage(chatId, BotPhrases.TripsFound + $" ({trips.Count}):");
                var photo = trip.Photo;
                var text = GetTripText(trip, userName);

                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить");

                if (trips.Count > 1)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
                }
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Далее"))
            {
                var index = trips.IndexOf(_currentTrip) + 1;
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var trip = trips[index];
                _currentTrip = trip;
                var photo = trip.Photo;
                var text = GetTripText(trip, userName);
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
                if (index == trips.Count - 1)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад");
                }
                var messageId = update.CallbackQuery.Message.Id;
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Назад"))
            {
                var index = trips.IndexOf(_currentTrip) - 1;
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var trip = trips[index];
                _currentTrip = trip;
                var photo = trip.Photo;
                var text = GetTripText(trip, userName);
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
                if (index == 0)
                {
                    inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Далее");
                }
                var messageId = update.CallbackQuery.Message.Id;
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Удалить"))
            {
                var tripToDelete = _currentTrip;
                var index = trips.IndexOf(tripToDelete);
                trips.Remove(tripToDelete);
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                await _botClient.DeleteMessage(chatId, messageId);
                if (trips.Count == 0)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                if (trips.Count == index)
                {
                    index--;
                }
                var trip = trips[index];
                _currentTrip = trip;
                var photo = trip.Photo;
                var text = GetTripText(trip, userName);
                var inlineMarkup = GetNavigationButtons(trips.Count, index);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
                //удалить из БД
            }
        }

        private static InlineKeyboardMarkup? GetNavigationButtons(int count, int index)
        {
            InlineKeyboardMarkup? inlineMarkup = null;
            if (count == 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить");
            }
            if (count > 1 && index == 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Далее");
            }
            if (count > 1 && index != 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
            }
            if (count > 1 && index == count - 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад");
            }
            return inlineMarkup;
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

        private string GetTripText(Trip trip, string userName)
        {
            var status = GetStatus(trip.Status);
            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + _post.User.UserName);
            return message.ToString();
        }

        private static string GetStatus(TripStatus status)
        {
            return status switch
            {
                TripStatus.New => "на рассмотрении администраицией.",
                TripStatus.Accepted => "одобрено администрацией.",
                TripStatus.Declined => "отклонена администраицей. Удалите пост и создайте новый.",
                TripStatus.OnTheWay => "в пути.",
                TripStatus.Ended => "завершена.",
                _ => "Unknown",
            };
        }

        private static List<Trip>? GetTrips(Post post) => post.Trips;

        private static Post? GetPost(Common.Model.User user)
        {
            var userName = user.UserName;
            return _posts.Where(x => x.User.UserName == userName).FirstOrDefault();
        }
    }
}
