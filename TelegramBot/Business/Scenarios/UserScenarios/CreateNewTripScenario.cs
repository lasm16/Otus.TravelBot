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
using TelegramBot.Business.Bot;

namespace TelegramBot.Business.Scenarios.UserScenarios
{
    public class CreateNewTripScenario : BaseScenario, IScenario
    {
        private Trip _trip = new();
        private int _confirmMessageId = 0;

        private readonly string _launchCommand = AppConfig.LaunchCommand;

        public CreateNewTripScenario(TelegramBotClient botClient, DataBase.Models.User user) : base(botClient)
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
            await SaveToDb();
            await BotClient.SendMessage(chatId, BotPhrases.Done);
            await BotClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null);
            UnsubscribeEvents();
            var scenario = new GreetingScenario(BotClient);
            scenario.Launch();
        }

        private async Task SaveToDb()
        {
            using var db = new ApplicationContext();
            db.Users.Attach(User);
            await db.Trips.AddAsync(_trip);
            await db.SaveChangesAsync();
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
            var userName = User.NickName;
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
            message.Append("Дата начала поездки: " + _trip.DateStart.ToShortDateString() + "\r\n");
            message.Append("Дата окончания поездки: " + _trip.DateEnd.ToShortDateString() + "\r\n");
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
            if (_trip.DateStart == DateTime.MinValue)
            {
                _ = DateTime.TryParse(inputText, out var result);
                _trip.DateStart = result;
                return (false, BotPhrases.EnterEndDate);
            }
            if (_trip.DateEnd == DateTime.MinValue)
            {
                _ = DateTime.TryParse(inputText, out var result);
                _trip.DateEnd = result;
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
            _trip.User = User;
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
