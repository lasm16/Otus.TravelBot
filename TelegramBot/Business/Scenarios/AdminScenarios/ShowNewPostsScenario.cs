using Common.Model.Bot;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Serilog;
using Telegram.Bot.Polling;
using Common.Data;
using TelegramBot.Business.Bot;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using DataBase.Models;
using DataBase;

namespace TelegramBot.Business.Scenarios.AdminScenarios
{
    public class ShowNewPostsScenario(TelegramBotClient botClient) : BaseScenario(botClient), IScenario
    {
        private int _currentTripIndex = 0;
        private int _confirmMessageId = 0;
        private int _messageIdForPostsCount = 0;
        private List<Trip> _trips => GetNewTrips();

        private List<string> _launchCommands = AppConfig.LaunchCommands;

        public void Launch() => SubscribeEvents();

        private async Task OnUpdate(Update update)
        {
            var button = update.CallbackQuery!.Data;
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
            var index = _currentTripIndex;
            var tripToDecline = _trips[index];
            await Decline(tripToDecline);
            if (_trips.Count == 0)
            {
                await BotClient.DeleteMessage(chatId, messageId, cancellationToken: BotClient.GlobalCancelToken);
                await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_trips.Count}):", cancellationToken: BotClient.GlobalCancelToken);
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            if (_trips.Count == index)
            {
                index--;
            }
            var trip = _trips[index];
            _currentTripIndex = index;
            var (text, photo) = GetTripTextWithPhoto(trip);
            var inlineMarkup = GetNavigationButtons(_trips.Count, index);
            await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.PostsFound + $" ({_trips.Count}):", cancellationToken: BotClient.GlobalCancelToken);

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task AcceptClick(long chatId, int messageId)
        {
            var index = _currentTripIndex;
            var tripToAccept = _trips[index];
            await Accept(tripToAccept);
            if (_trips.Count == 0)
            {
                await BotClient.DeleteMessage(chatId, messageId, cancellationToken: BotClient.GlobalCancelToken);
                await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_trips.Count}):", cancellationToken: BotClient.GlobalCancelToken);
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            if (_trips.Count == index)
            {
                index--;
            }
            var trip = _trips[index];
            _currentTripIndex = index;
            var (text, photo) = GetTripTextWithPhoto(trip);
            var inlineMarkup = GetNavigationButtons(_trips.Count, index);
            await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.PostsFound + $" ({_trips.Count}):", cancellationToken: BotClient.GlobalCancelToken);

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task DeclineAllClick(long chatId, int messageId)
        {
            await DeclineAll();
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null, cancellationToken: BotClient.GlobalCancelToken);
            await BotClient.SendMessage(chatId, BotPhrases.AllTripsDeclined, cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task AcceptAllClick(long chatId, int messageId)
        {
            await AcceptAll();
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null, cancellationToken: BotClient.GlobalCancelToken);
            await BotClient.SendMessage(chatId, BotPhrases.AllTripsAccepted, cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task AcceptAll()
        {
            var db = new ApplicationContext();
            foreach (var trip in _trips)
            {
                trip.Status = TripStatus.Accepted;
                db.Trips.Update(trip);
            }
            await db.SaveChangesAsync(cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task DeclineAll()
        {
            var db = new ApplicationContext();
            foreach (var trip in _trips)
            {
                trip.Status = TripStatus.Declined;
                db.Trips.Update(trip);
            }
            await db.SaveChangesAsync(cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task Decline(Trip trip)
        {
            trip.Status = TripStatus.Declined;
            var db = new ApplicationContext();
            db.Trips.Update(trip);
            await db.SaveChangesAsync(cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task Accept(Trip trip)
        {
            trip.Status = TripStatus.Accepted;
            var db = new ApplicationContext();
            db.Trips.Update(trip);
            await db.SaveChangesAsync(cancellationToken: BotClient.GlobalCancelToken);
        }

        private static List<Trip> GetNewTrips()
        {
            using var db = new ApplicationContext();
            var trips = db.Trips.ToList();
            return [.. trips.Where(x => x.Status == TripStatus.New).OrderBy(x => x.DateCreated)];
        }

        private async Task ShowClick(long chatId, int messageId)
        {
            var newTripsCount = _trips.Count;
            if (newTripsCount == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            var trip = _trips.FirstOrDefault();
            _currentTripIndex = 0;
            var (text, photo) = GetTripTextWithPhoto(trip!);

            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить");

            if (_trips.Count > 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Принять", "Отклонить", "Далее");
            }
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null, cancellationToken: BotClient.GlobalCancelToken);
            var botMessage = await BotClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var index = _currentTripIndex - 1;
            var trip = _trips[index];
            _currentTripIndex = index;
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
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task NextClick(long chatId, int messageId)
        {
            var index = _currentTripIndex + 1;
            var trip = _trips[index];
            _currentTripIndex = index;
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
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task NewPostsClick(long chatId)
        {
            var newTripsCount = _trips.Count;
            if (newTripsCount == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Посмотреть", "Принять все", "Отклонить все");
            var botMessage = await BotClient.SendMessage(chatId, BotPhrases.PostsFound + $" ({newTripsCount}):", replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
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

        private static (string text, string photo) GetTripTextWithPhoto(Trip trip)
        {
            var userId = trip.UserId;
            var userName = GetUserName(userId);
            var status = Helper.GetStatus(TripStatus.New);

            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + ", " + trip.Country + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart.ToShortDateString() + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd.ToShortDateString() + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + userName);

            var text = message.ToString();
            var photo = trip.Photo;
            return (text!, photo!);
        }

        private static string? GetUserName(long userId)
        {
            var db = new ApplicationContext();
            return db.Users.Where(x => x.Id == userId).FirstOrDefault()!.NickName;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(exception.Message, exception.StackTrace, exception.InnerException);
                Log.Debug(exception.Message, exception.StackTrace, exception.InnerException);
            }, cancellationToken: BotClient.GlobalCancelToken);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var inputLine = message.Text;
            var chatId = message.Chat.Id;
            if (inputLine is null)
            {
                return;
            }
            if (!_launchCommands.Contains(inputLine))
            {
                Log.Error("Некорректно указан сценарий!");
                await BotClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            if (_confirmMessageId != 0)
            {
                try
                {
                    await BotClient.EditMessageReplyMarkup(chatId, _confirmMessageId, null, cancellationToken: BotClient.GlobalCancelToken);
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
