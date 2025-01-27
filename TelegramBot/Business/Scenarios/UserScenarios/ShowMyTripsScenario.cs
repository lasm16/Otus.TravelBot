using Common.Data;
using Common.Model.Bot;
using DataBase;
using DataBase.Models;
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
    public class ShowMyTripsScenario : BaseScenario, IScenario
    {
        private int _currentTripIndex = 0;
        private int _confirmMessageId = 0;
        private int _messageIdForPostsCount = 0;
        private List<Trip> _myTrips => GetTrips();

        private List<string> _launchCommands = AppConfig.LaunchCommands;

        public ShowMyTripsScenario(TelegramBotClient botClient, DataBase.Models.User user) : base(botClient)
        {
            BotClient = botClient;
            User = user;
        }

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
            var userName = User!.NickName;
            var index = _currentTripIndex;
            var tripToDelete = _myTrips[index];
            await DeleteFromDb(tripToDelete);
            if (_myTrips.Count == 0)
            {
                await BotClient.DeleteMessage(chatId, messageId, cancellationToken: BotClient.GlobalCancelToken);
                await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_myTrips.Count}):", cancellationToken: BotClient.GlobalCancelToken);
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            if (_myTrips.Count == index)
            {
                index--;
            }
            var trip = _myTrips[index];
            _currentTripIndex = index;
            var photo = trip.Photo;
            var text = GetTripText(trip, userName!);
            var inlineMarkup = GetNavigationButtons(_myTrips.Count, index);
            var media = new InputMediaPhoto(photo!)
            {
                Caption = text,
            };
            await BotClient.EditMessageText(chatId, _messageIdForPostsCount, BotPhrases.TripsFound + $" ({_myTrips.Count}):", cancellationToken: BotClient.GlobalCancelToken);
            await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
        }

        private static async Task DeleteFromDb(Trip trip)
        {
            using var db = new ApplicationContext();
            db.Trips.Remove(trip);
            await db.SaveChangesAsync();
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var userName = User!.NickName;
            var index = _currentTripIndex - 1;
            var trip = _myTrips[index];
            _currentTripIndex = index;
            var photo = trip.Photo;
            var text = GetTripText(trip, userName!);
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Далее");
            }
            var media = new InputMediaPhoto(photo!)
            {
                Caption = text,
            };
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task NextClick(long chatId, int messageId)
        {
            var userName = User!.NickName;
            var index = _currentTripIndex + 1;
            var trip = _myTrips[index];
            _currentTripIndex = index;
            var photo = trip.Photo;
            var text = GetTripText(trip, userName!);
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
            if (index == _myTrips.Count - 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Назад");
            }
            var media = new InputMediaPhoto(photo!)
            {
                Caption = text,
            };
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup, cancellationToken: BotClient.GlobalCancelToken);
            _confirmMessageId = botMessage.MessageId;
        }

        private async Task MyTripsClick(long chatId)
        {
            var userName = User!.NickName;
            if (_myTrips.Count == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound, cancellationToken: BotClient.GlobalCancelToken);
                return;
            }
            var trip = _myTrips.FirstOrDefault();
            _currentTripIndex = 0;
            var photo = trip!.Photo;
            var text = GetTripText(trip, userName!);

            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить");

            if (_myTrips.Count > 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Далее");
            }
            var botMessage1 = await BotClient.SendMessage(chatId, BotPhrases.TripsFound + $" ({_myTrips.Count}):", cancellationToken: BotClient.GlobalCancelToken);
            _messageIdForPostsCount = botMessage1.MessageId;
            var botMessage2 = await BotClient.SendPhoto(chatId, photo!, text, replyMarkup: inlineMarkup);
            _confirmMessageId = botMessage2.MessageId;
        }

        private static InlineKeyboardMarkup? GetNavigationButtons(int count, int index)
        {
            InlineKeyboardMarkup? inlineMarkup = null;
            if (count == 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить");
            }
            if (count > 1 && index == 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Далее");
            }
            if (count > 1 && index != 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Назад", "Далее");
            }
            if (count > 1 && index == count - 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Удалить", "Назад");
            }
            return inlineMarkup;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            await Task.Run(() =>
            {
                var message = exception.Message;
                var inner = exception.InnerException;
                Console.WriteLine(message);
                Console.WriteLine(inner);
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
            var scenario = new GreetingScenario(BotClient, User!);
            scenario.Launch();
        }

        private void SubscribeEvents()
        {
            BotClient.OnError += OnError;
            BotClient.OnMessage += OnMessage;
            BotClient.OnUpdate += OnUpdate;
        }

        private void UnsubscribeEvents()
        {
            BotClient.OnError -= OnError;
            BotClient.OnMessage -= OnMessage;
            BotClient.OnUpdate -= OnUpdate;
        }

        private static string GetTripText(Trip trip, string userName)
        {
            var status = Helper.GetStatus(trip.Status);
            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + ", " + trip.Country + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart.ToShortDateString() + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd.ToShortDateString() + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + userName);
            return message.ToString();
        }

        private List<Trip> GetTrips()
        {
            using var db = new ApplicationContext();
            var trips = db.Trips.ToList();
            return [.. trips.Where(x => x.UserId == User!.Id).OrderBy(x => x.DateCreated)];
        }
    }
}
