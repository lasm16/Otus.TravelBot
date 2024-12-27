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
    public class CreateNewTripScenario : BaseScenario, IScenario
    {
        private Trip _trip = new();
        private int _confirmMessageId = 0;

        private readonly string _launchCommand = AppConfig.LaunchCommand;

        public CreateNewTripScenario(TelegramBotClient botClient, Common.Model.User user) : base(botClient)
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
            await ButtonClick(button, chatId, messageId);
        }

        private async Task SendMessageForNewTrip(long chatId)
        {
            var messageList = new List<string>()
            {
                BotPhrases.Agreement,
                BotPhrases.EnterCity
            };
            foreach (var message in messageList)
            {
                await BotClient.SendMessage(chatId, message);
            }
        }

        private async Task EditClick(long chatId)
        {
            _trip = new Trip();
            await BotClient.SendMessage(chatId, BotPhrases.EnterCity);
        }

        private async Task ButtonClick(string? button, long chatId, int messageId)
        {
            switch (button)
            {
                case "Готово":
                    await FinishClick(chatId, messageId);
                    break;
                case "Редактировать":
                    await EditClick(chatId);
                    break;
                case "Новая поездка":
                    await SendMessageForNewTrip(chatId);
                    break;
                default:
                    return;
            }
        }

        private async Task FinishClick(long chatId, int messageId)
        {
            await SaveToFile();
            await BotClient.SendMessage(chatId, BotPhrases.Done);
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            UnsubscribeEvents();
            var scenario = new GreetingScenario(BotClient);
            scenario.Launch();
        }

        // удалю, когда будет БД
        private async Task SaveToFile()
        {
            var post = new Post
            {
                User = User,
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
            Console.WriteLine(exception.Message, exception.StackTrace);
            Log.Debug(exception.Message, exception.StackTrace);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var chatId = message.Chat.Id;
            var inputLine = message.Text ?? message.Photo!.Last().FileId;
            if (inputLine == null)
            {
                return;
            }
            if (inputLine.Equals(_launchCommand))
            {
                if (_confirmMessageId != 0)
                {
                    await BotClient.EditMessageReplyMarkup(chatId, _confirmMessageId, null);
                }
                UnsubscribeEvents();
                var scenario = new GreetingScenario(BotClient);
                scenario.Launch();
                return;
            }
            var (isFilled, outPutLine) = FillTrip(inputLine);
            if (isFilled)
            {
                await SendMessageWithInlineKeyboard(chatId, outPutLine);
                return;
            }
            await BotClient.SendMessage(chatId, outPutLine);
        }

        private async Task SendMessageWithInlineKeyboard(long chatId, string outPutLine)
        {
            var inlineMarkup = Helper.GetInlineKeyboardMarkup("Редактировать", "Готово");
            var photo = _trip.Photo;
            var userName = User.UserName;
            var tripText = GetTripText(outPutLine, userName);
            try
            {
                var botMessage = await BotClient.SendPhoto(chatId, photo, tripText, replyMarkup: inlineMarkup);
                _confirmMessageId = botMessage.Id;
            }
            catch (ApiRequestException e)
            {
                Log.Error(e.Message, e.StackTrace);
                await BotClient.SendMessage(chatId, BotPhrases.UploadPhotoError);
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
