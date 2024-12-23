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

namespace TelegramBot.Business.Scenarios.UserScenarios
{
    public class ShowTripsScenario(TelegramBotClient botClient) : IScenario
    {
        private int counter = 0;
        private List<Trip>? _trips = Repository.Trips;
        private readonly TelegramBotClient _botClient = botClient;

        public void Launch()
        {
            SubscriveEvents();
        }

        private async Task OnUpdate(Update update)
        {

            if (update.CallbackQuery.Data.Equals("Мои поездки"))
            {
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var trip = _trips.FirstOrDefault();
                if (trip == null)
                {
                    await _botClient.SendMessage(chatId, BotPhrases.TripsNotFound);
                    return;
                }
                await _botClient.SendMessage(chatId, BotPhrases.TripsFound);
                var photo = trip.Photo;
                var userName = update.CallbackQuery!.From!.Username; // Переделать на получение из файла/БД
                var text = GetTripText(trip, userName);

                InlineKeyboardMarkup inlineMarkup = null;
                if (_trips.Count > 1)
                {
                    inlineMarkup = new InlineKeyboardMarkup()
                                    .AddButton("Далее", "Далее");
                }
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
                var messageId = update.CallbackQuery.Message.Id;
            }
            if (update.CallbackQuery.Data.Equals("Далее"))
            {
                counter++;
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var trip = _trips[counter];
                var photo = trip.Photo;
                var userName = update.CallbackQuery!.From!.Username; // Переделать на получение из файла/БД
                var text = GetTripText(trip, userName);
                var inlineMarkup = new InlineKeyboardMarkup()
                    .AddButton("Назад", "Назад")
                    .AddButton("Далее", "Далее");
                if (counter == _trips.Count - 1)
                {
                    inlineMarkup = new InlineKeyboardMarkup()
                                    .AddButton("Назад", "Назад");
                }
                var messageId = update.CallbackQuery.Message.Id;
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }
            if (update.CallbackQuery.Data.Equals("Назад"))
            {
                counter--;
                var chatId = update.CallbackQuery!.Message!.Chat.Id;
                var trip = _trips[counter];
                var photo = trip.Photo;
                var userName = update.CallbackQuery!.From!.Username; // Переделать на получение из файла/БД
                var text = GetTripText(trip, userName);
                var inlineMarkup = new InlineKeyboardMarkup()
                    .AddButton("Назад", "Назад")
                    .AddButton("Далее", "Далее");
                if (counter == 0)
                {
                    inlineMarkup = new InlineKeyboardMarkup()
                                    .AddButton("Далее", "Далее");
                }
                var messageId = update.CallbackQuery.Message.Id;
                await _botClient.DeleteMessage(chatId, messageId);
                await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
            }

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

        private void SubscriveEvents()
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
            var message = new StringBuilder();
            message.Append("Планирую посетить: " + trip.City + "\r\n");
            message.Append("Дата начала поездки: " + trip.DateStart + "\r\n");
            message.Append("Дата окончания поездки: " + trip.DateEnd + "\r\n");
            message.Append("Описание: \r\n" + trip.Description + "\r\n");
            message.Append("@" + userName);
            return message.ToString();
        }
    }
}
