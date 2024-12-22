﻿using Common.Data;
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
    internal class CreateNewTripScenario(TelegramBotClient botClient) : IScenario
    {
        private Trip _trip = new();
        private TelegramBotClient _botClient = botClient;

        public void DoAction()
        {
            _botClient.OnError += OnError;
            _botClient.OnMessage += OnMessage;
            _botClient.OnUpdate += OnUpdate;
        }

        private async Task OnUpdate(Update update)
        {
            if (update.CallbackQuery!.Data == "Готово")
            {
                // будет сохранение в БД
                var chatId = update.CallbackQuery.Message!.Chat.Id;
                var messageId = update.CallbackQuery.Message.Id;
                await _botClient.SendMessage(chatId, BotPhrases.Done);
                await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null); // пытаюсь скрыть клавиатуру, не работает. Почему?
                return;
            }
            if (update.CallbackQuery.Data == "Редактировать")
            {
                _trip = new Trip();
            }
            var messageList = new List<string>()
            {
                GetAgreementMessage(),
                BotPhrases.EnterCity
            };
            foreach (var message in messageList)
            {
                await _botClient.SendMessage(update.CallbackQuery.Message!.Chat.Id, message);
            }
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
            Log.Debug(exception.Message);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            var inputLine = message.Text ?? message.Photo!.Last().FileId;
            if (inputLine == null)
            {
                return;
            }
            var (isFilled, outPutLine) = FillTrip(inputLine);
            if (isFilled)
            {
                var inlineMarkup = new InlineKeyboardMarkup()
                    .AddButton("Готово", "Готово")
                    .AddButton("Редактировать", "Редактировать");

                var userName = message.Chat.Username;
                var tripText = GetTripText(outPutLine, userName!);
                var photo = _trip.Photo;
                await _botClient.SendPhoto(message.Chat.Id, photo, tripText, replyMarkup: inlineMarkup);
                return;
            }
            await _botClient.SendMessage(message.Chat.Id, outPutLine);
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
            _trip.Status = TripStatus.Review;
            return (true, BotPhrases.ConfirmTrip);
        }
    }
}
