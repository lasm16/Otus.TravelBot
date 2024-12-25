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
    public class FindFellowScenario(TelegramBotClient botClient, Common.Model.User user) : IScenario
    {
        private Common.Model.User _user = user;
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
            var isDate = DateTime.TryParse(inputLine, out var date);
            var chatId = message.Chat.Id;
            if (isDate)
            {
            }

            await SearchTripsWithCityNames(inputLine, chatId);
            //if (!inputLine.Equals("/start"))
            //{
            //    Log.Error("Некорректно указан сценарий!");
            //    await _botClient.SendMessage(message.Chat.Id, BotPhrases.UnknownCommand);
            //    return;
            //}
            //UnsubscribeEvents();
            //var scenario = new GreetingScenario(_botClient);
            //scenario.Launch();
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task SearchTripsWithCityNames(string inputLine, long chatId)
        {
            var postsWithoutCurrentUser = _posts.Where(x => x.User.UserName != _user.UserName).ToList();
            //var trips = postsWithoutCurrentUser.
            var trips = new List<Trip>();
            var trip = new Trip();
            await _botClient.SendMessage(chatId, BotPhrases.TripsFound + $" ({postsWithoutCurrentUser.Count}):");
            var photo = trip.Photo;
            var userName = "";
            var text = GetTripText(trip, userName);

            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Удалить");

            if (trips.Count > 1)
            {
                inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Далее");
            }
            await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineMarkup);
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
            if (button == "Найти попутчика")
            {
                var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("По городу", "По дате");
                await Click(chatId, messageId, BotPhrases.SearchType, inlineMarkup);
                return;
            }
            if (button == "По городу")
            {
                await Click(chatId, messageId, BotPhrases.SearchCity, null);
                return;
            }
            if (button == "По дате")
            {
                await Click(chatId, messageId, BotPhrases.SearchDate, null);
                return;
            }
        }

        private async Task Click(long chatId, int messageId, string message, InlineKeyboardMarkup? inlineKeyboard)
        {
            await _botClient.SendMessage(chatId, message, replyMarkup: inlineKeyboard);
            await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
        }
    }
}
