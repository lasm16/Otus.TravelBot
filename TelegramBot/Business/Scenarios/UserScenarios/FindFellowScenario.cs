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
    public class FindFellowScenario : BaseScenario, IScenario
    {
        private int _currentTripIndex = 0;
        private int _currentMessageId = 0;
        private List<Trip> _trips = GetTrips();
        private List<Trip> _searchedTrips = [];

        private List<string> _launchCommands = AppConfig.LaunchCommands;

        public FindFellowScenario(TelegramBotClient botClient, DataBase.Models.User user) : base(botClient)
        {
            BotClient = botClient;
            User = user;
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
            if (_trips.Count == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
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
            if (_launchCommands.Contains(inputLine))
            {
                if (_currentMessageId != 0)
                {
                    try
                    {
                        await BotClient.EditMessageReplyMarkup(chatId, _currentMessageId, null);
                    }
                    catch (ApiRequestException e)
                    {
                        Log.Error(e.Message, e.StackTrace);
                    }
                }
                UnsubscribeEvents();
                var scenario = new GreetingScenario(BotClient);
                scenario.Launch();
                return;
            }
            if (_currentMessageId != 0)
            {
                try
                {
                    await BotClient.EditMessageReplyMarkup(chatId, _currentMessageId, null);
                }
                catch (ApiRequestException e)
                {
                    Log.Error(e.Message, e.StackTrace);
                }
                _currentMessageId = 0;
            }
            await SearchTripsWithInputLine(inputLine, chatId);
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message, exception.StackTrace, exception.InnerException);
            Log.Debug(exception.Message, exception.StackTrace, exception.InnerException);
        }

        private async Task SearchTripsWithInputLine(string inputLine, long chatId)
        {
            _searchedTrips = GetTripsWithFilter(inputLine);

            if (_searchedTrips.Count == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }
            _currentTripIndex = 0;
            var trip = _searchedTrips[_currentTripIndex];
            var (text, photo) = GetTripText(trip);

            InlineKeyboardMarkup inlineMarkup = null;

            if (_searchedTrips.Count > 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Далее");
            }
            var message = BotPhrases.TripsFound + $" ({_searchedTrips.Count}):";
            await BotClient.SendMessage(chatId, message);

            try
            {
                await BotClient.DeleteMessage(chatId, _currentMessageId);
            }
            catch (ApiRequestException e)
            {
                Log.Error(e.Message, e.StackTrace);
                Console.WriteLine(e.Message, e.StackTrace);
            }
            var botMessage = BotClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.Result.MessageId;
        }

        private static (string text, string photo) GetTripText(Trip trip)
        {
            var userId = trip.UserId;
            var status = Helper.GetStatus(trip.Status);
            var userName = GetUserName(userId);

            var message = new StringBuilder();
            message.Append("Статус поездки: " + status + "\r\n");
            message.Append("Планирую посетить: " + trip.City + ", " + trip.Country + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart.ToShortDateString() + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd.ToShortDateString() + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + userName);

            var text = message.ToString();
            var photo = trip.Photo;
            return (text, photo);
        }

        private static List<Trip> GetTrips()
        {
            using var db = new ApplicationContext();
            var trips = db.Trips.ToList();
            return [.. trips.Where(x => x.Status == TripStatus.Accepted || x.Status == TripStatus.OnTheWay).OrderBy(x => x.DateStart)];
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
            _trips = GetTrips();
            if (_trips.Count == 0)
            {
                await BotClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                return;
            }

            var trip = _trips[_currentTripIndex];
            var (text, photo) = GetTripText(trip);

            InlineKeyboardMarkup inlineMarkup = null;

            if (_trips.Count > 1)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Далее");
            }

            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            var message = BotPhrases.TripsFound + $" ({_trips.Count}):";
            await BotClient.SendMessage(chatId, message);
            var botMessage = await BotClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;
        }

        private async Task PreviousClick(long chatId, int messageId)
        {
            var index = _currentTripIndex - 1;
            Trip trip;
            if (_searchedTrips.Count != 0)
            {
                trip = _searchedTrips[index];
            }
            else
            {
                trip = _trips[index];
            }
            _currentTripIndex = index;
            var (text, photo) = GetTripText(trip);
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Назад", "Далее");
            if (index == 0)
            {
                inlineMarkup = Helper.GetInlineKeyboardMarkup("Далее");
            }
            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;

        }

        private async Task NextClick(long chatId, int messageId)
        {
            var index = _currentTripIndex + 1;
            Trip trip;
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Назад", "Далее");

            if (_searchedTrips.Count != 0)
            {
                trip = _searchedTrips[index];
                if (index == _searchedTrips.Count - 1)
                {
                    inlineMarkup = Helper.GetInlineKeyboardMarkup("Назад");
                }
            }
            else
            {
                trip = _trips[index];
                if (index == _trips.Count - 1)
                {
                    inlineMarkup = Helper.GetInlineKeyboardMarkup("Назад");
                }
            }
            _currentTripIndex = index;
            var (text, photo) = GetTripText(trip);

            var media = new InputMediaPhoto(photo)
            {
                Caption = text,
            };
            var botMessage = await BotClient.EditMessageMedia(chatId, messageId, media, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;
        }

        private async Task FindFellowClick(long chatId, int messageId, string message)
        {
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Показать все");
            var botMessage = await BotClient.SendMessage(chatId, message, replyMarkup: inlineMarkup);
            _currentMessageId = botMessage.MessageId;
            await BotClient.EditMessageReplyMarkup(chatId, messageId);
        }

        private static string? GetUserName(long userId)
        {
            var db = new ApplicationContext();
            return db.Users.Where(x => x.Id == userId).FirstOrDefault()!.NickName;
        }

        private List<Trip> GetTripsWithFilter(string inputLine)
        {
            var isDate = DateTime.TryParse(inputLine, out var date);
            if (isDate)
            {
                return [.. _trips.Where(x => x.DateStart.ToShortDateString().Equals(inputLine)).OrderBy(x => x.DateStart)];
            }
            else
            {
                return [.. _trips.Where(x => x.City.Equals(inputLine) || x.Country.Equals(inputLine)).OrderBy(x => x.DateStart)];
            }
        }
    }
}
