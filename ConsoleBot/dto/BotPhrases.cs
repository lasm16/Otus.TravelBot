using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.dto
{
    public static class BotPhrases
    {
        public const string Welcome = "Добро подаловать в прототип бота для ТГ";
        public const string Role = "Выберите свою роль: админ или пользователь";
        public const string AvailableActions = "Список доступных действий:";
        public const string UnavailableActions = "Такого действия нет!";

        public const string Agreement = "Для того чтобы ваш пост был размещён в группе он должен соответствовать следующему описанию:";
        public const string SuggestStartDate = "1) Предполагаемую дату начала поездки в формате ДД.ММ.ГГГГ: ";
        public const string SuggestEndDate = "2) Предполагаемую дату окончания поездки в формате ДД.ММ.ГГГГ: ";
        public const string Description = "3) краткое описание плана по вашему путешествию.";
        public const string Photo = "4) ваше фото.";
        public const string LinkVk = "5) реальный аккаунт ВК (посты от фейк аккаунтов и закрытыми профилями не публикуем. Нужно только для проверки админу)";
        public const string Done = "Почти все готово! После проверки сообщения админстрацией ваш пост будет опубликован!";

        public const string SuggestDate = "Введите дату предполагаемой поездки";
        public const string FindTrips = "Нашел твои поездки:";

        public const string PostForDelete = "Введите id поста, который хотите удалить:";
        public const string PostForUpdate = "Введите id поста, который хотите обновить:";
        public const string PostForVip = "Введите id поста, который хотите VIP:";
    }
}
