using Common.Data;
using Common.Model;
using Common.Model.Bot;
using Serilog;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Business.Bot;
using TelegramBot.Business.Utils;

namespace TelegramBot.Business.Scenarios.UserScenarios
{
    public class CreateNewTripScenario(TelegramBotClient botClient, Common.Model.User user) : IScenario
    {
        private Trip _trip = new();
        private readonly Common.Model.User _user = user;
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

        private async Task SendMessageForNewTrip(long chatId)
        {
            var messageList = new List<string>()
            {
                GetAgreementMessage(),
                BotPhrases.EnterCity
            };
            foreach (var message in messageList)
            {
                await _botClient.SendMessage(chatId, message);
            }
        }

        private async Task ButtonClick(string? button, long chatId, int messageId)
        {
            switch (button)
            {
                case "Готово":
                    await FinishClick(chatId, messageId);
                    break;
                case "Редактировать":
                    EditClick();
                    await SendMessageForNewTrip(chatId);
                    break;
                case "Новая поездка":
                    await SendMessageForNewTrip(chatId);
                    break;
                default:
                    return;
            }
        }

        private void EditClick() => _trip = new Trip();

        private async Task FinishClick(long chatId, int messageId)
        {
            await SaveToFile();
            await _botClient.SendMessage(chatId, BotPhrases.Done);
            await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            UnsubscribeEvents();
            var scenario = new GreetingScenario(_botClient);
            scenario.Launch();
        }

        // удалю, когда будет БД
        private async Task SaveToFile()
        {
            var post = new Post
            {
                User = _user,
                Trips = []
            };
            post.Trips.Add(_trip);
            var options = JsonUtils.GetSerializerOptions();
            var json = JsonSerializer.Serialize(post, options);
            using var writer = new StreamWriter("new_posts.json", true, Encoding.UTF8);
            await writer.WriteLineAsync(json);
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var chatId = message.Chat.Id;
            var inputLine = message.Text ?? message.Photo!.Last().FileId;
            if (inputLine == null)
            {
                return;
            }
            if (inputLine.Equals("/start"))
            {
                UnsubscribeEvents();
                var scenario = new GreetingScenario(_botClient);
                scenario.Launch();
                return;
            }
            var (isFilled, outPutLine) = FillTrip(inputLine);
            if (isFilled)
            {
                await SendMessageWithInlineKeyboard(chatId, outPutLine);
                return;
            }
            await _botClient.SendMessage(chatId, outPutLine);
        }

        private async Task SendMessageWithInlineKeyboard(long chatId, string outPutLine)
        {
            var inlineMarkup = TelegramBotImpl.GetInlineKeyboardMarkup("Готово", "Редактировать");
            var photo = _trip.Photo;
            var userName = _user.UserName;
            var tripText = GetTripText(outPutLine, userName);
            try
            {
                await _botClient.SendPhoto(chatId, photo, tripText, replyMarkup: inlineMarkup);
            }
            catch (ApiRequestException e)
            {
                Log.Error(e.Message);
                await _botClient.SendMessage(chatId, BotPhrases.UploadPhotoError);
                _trip.Photo = null;
            }
        }

        private string GetTripText(string text, string userName)
        {
            var message = new StringBuilder(text + "\r\n");
            message.Append("Планирую посетить: " + _trip.City + "\r\n");
            message.Append("Дата начала поездки: " + _trip.DateStart + "\r\n");
            message.Append("Дата окончания поездки: " + _trip.DateEnd + "\r\n");
            message.Append("Описание: \r\n" + _trip.Description + "\r\n");
            message.Append("@" + userName);
            return message.ToString();
        }

        private static string GetAgreementMessage()
        {
            var message = new StringBuilder();
            message.Append(BotPhrases.Agreement);
            message.Append(BotPhrases.SuggestCity);
            message.Append(BotPhrases.SuggestStartDate);
            message.Append(BotPhrases.SuggestEndDate);
            message.Append(BotPhrases.SuggestDescription);
            message.Append(BotPhrases.SuggestPhoto);
            return message.ToString();
        }

        private (bool isFilled, string outPutLine) FillTrip(string inputText)
        {
            if (_trip.City == null)
            {
                _trip.City = inputText;
                return (false, BotPhrases.EnterStartDate);
            }
            if (_trip.DateStart == null)
            {
                _trip.DateStart = inputText;
                return (false, BotPhrases.EnterEndDate);
            }
            if (_trip.DateEnd == null)
            {
                _trip.DateEnd = inputText;
                return (false, BotPhrases.EnterDescription);
            }
            if (_trip.Description == null)
            {
                _trip.Description = inputText;
                return (false, BotPhrases.EnterPhoto);
            }
            if (_trip.Photo == null)
            {
                _trip.Photo = inputText;
            }
            _trip.Id = Guid.NewGuid();
            _trip.Status = TripStatus.New;
            return (true, BotPhrases.ConfirmTrip);
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
