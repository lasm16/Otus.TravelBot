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
    public class ShowMyTripsScenario(TelegramBotClient botClient, Common.Model.User user) : IScenario
    {
        private Trip? _currentTrip;
        private int _confirmMessageId = 0;
        private Post? _post = GetPost(user);
        private int _messageIdForPostsCount = 0;
        private readonly Common.Model.User _user = user;
        private static List<Post> _posts = Repository.Posts;
        private readonly TelegramBotClient _botClient = botClient;

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
                case "Мои поездки":
                    await MyTripsClick(chatId);
                    break;
                case "Далее":
                    await NextClick(chatId, messageId);
                    break;
                case "Назад":
                    await PreviousClick(chatId, messageId);
                    break;
                case "Удалить":
                    await DeleteClick(chatId, messageId);
                    break;
                default:
                    return;
            }
        }

        private async Task DeleteClick(long chatId, int messageId)
        {
            var userName = _user.UserName;
            var trips = GetTrips(_post);
            var tripToDelete = _currentTrip;
            var index = trips.IndexOf(tripToDelete);
            trips.Remove(tripToDelete); // Заменить на удаление из БД
            if (trips.Count == 0)
            {
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({trips.Count}):");
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
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await _botClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({trips.Count}):");
            await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            //удалить из БД
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var userName = _user.UserName;
            var trips = GetTrips(_post);
            var index = trips.IndexOf(_currentTrip) - 1;
            var trip = trips[index];
            _currentTrip = trip;
            var photo = trip.Photo;
            var text = GetTripText(trip, userName);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Далее");
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
            var userName = _user.UserName;
            var trips = GetTrips(_post);
            var index = trips.IndexOf(_currentTrip) + 1;
            var trip = trips[index];
            _currentTrip = trip;
            var photo = trip.Photo;
            var text = GetTripText(trip, userName);
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
            if (index == trips.Count - 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Назад");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await _botClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task MyTripsClick(long chatId)
        {
            var userName = _user.UserName;
            var trips = GetTrips(_post);
            if (trips.Count == 0)
            {
                await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            var trip = trips[0];
            _currentTrip = trip;
            var photo = trip.Photo;
            var text = GetTripText(trip, userName);

            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить");

            if (trips.Count > 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить", "Далее");
            }
            var botMessage1 = await _botClient.SendMessage(chatId, BotPhrases.TripsFound + $" ({trips.Count}):");
            _messageIdForPostsCount = botMessage1.MessageId;
            var botMessage2 = await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage2.MessageId;
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
            var inputLine = message.Text;
            var chatId = message.Chat.Id;
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
            if (_confirmMessageId != 0)
            {
                try
                {
                    await _botClient.EditMessageReplyMarkup(chatId, _confirmMessageId, null);
                }
                catch (ApiRequestException ex)
                {
                    Log.Error(ex.Message);
                    Console.WriteLine(ex.Message);
                }
            }
            UnsubscribeEvents();
            var scenario = new GreetingScenario(_botClient);
            scenario.Launch();
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

        private static string GetTripText(Trip trip, string userName)
        {
            var status = TelegramBotImpl.GetStatus(trip.Status);
            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + userName);
            return message.ToString();
        }

        private static List<Trip>? GetTrips(Post post) => post.Trips;

        private static Post? GetPost(Common.Model.User user)
        {
            var userName = user.UserName;
            return _posts.Where(x => x.User.UserName == userName).FirstOrDefault();
        }
    }
}
