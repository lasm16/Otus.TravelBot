using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Business.Bot
{
    public class TelegramBotImpl(TelegramBotClient botClient)
    {
        private readonly TelegramBotClient _botClient = botClient;

        public async Task SendMessage(ChatId chatId, string text)
        {
            await _botClient.SendMessage(chatId, text);
        }

        public async Task SendMessageWithInlineKeyboard(ChatId chatId, string text, InlineKeyboardMarkup inlineKeyboard)
        {
            await _botClient.SendMessage(chatId, text, replyMarkup: inlineKeyboard);
        }

        public async Task HideInlineKeyboardInMessage(ChatId chatId, int messageId)
        {
            await _botClient.EditMessageReplyMarkup(chatId, messageId, null);
        }

        public async Task SendPhoto(ChatId chatId, string photo, string text, InlineKeyboardMarkup inlineKeyboard)
        {
            await _botClient.SendPhoto(chatId, photo, text, replyMarkup: inlineKeyboard);
        }

        public static InlineKeyboardMarkup GetInlineKeyboardMarkup(params string[] buttonsName)
        {
            var inlineMarkup = new InlineKeyboardMarkup();
            foreach (var button in buttonsName)
            {
                inlineMarkup.AddButton(button, button);
            }
            return inlineMarkup;
        }
    }
}
