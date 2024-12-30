namespace Common.Data
{
    public class BotPhrases
    {
        public const string UnknownCommand = "Я не знаю этой команды...";

        public const string Greeting1 = "Приветствую тебя, ";
        public const string Greeting2 = "! Ты можешь выложить пост о планируемой поездке или найти попутчика.";

        public const string Agreement =
            "Для того чтобы ваш пост был размещён, он должен соответствовать следующему описанию: \r\n" +
            "1) Место, которое планируете посетить. \r\n" +
            "2) Предполагаемая дата начала поездки. \r\n" +
            "3) Предполагаемая дата окончания поездки. \r\n" +
            "4) Краткое описание плана вашего путешествия. \r\n" +
            "5) Ваше фото. \r\n";

        public const string EnterCity = "Введите город, который планируете посетить: \r\n";
        public const string EnterCountry = "Введите страну, которую планируете посетить: \r\n";
        public const string EnterStartDate = "Введите предполагаемую дату начала поездки в формате ДД.ММ.ГГГГ: \r\n";
        public const string EnterEndDate = "Введите предполагаемую дату окончания поездки в формате ДД.ММ.ГГГГ: \r\n";
        public const string EnterDescription = "Введите краткое описание плана вашего путешествия: \r\n";
        public const string EnterPhoto = "Отправьте ваше фото: \r\n";
        public const string ConfirmTrip = "Вот ваш пост! Подтвердите или отредактируйте его:";
        public const string Done = "Почти все готово! После проверки сообщения админстрацией ваш пост будет опубликован!";

        public const string TripsFound = "Нашел поездки";
        public const string TripsNotFound = "🤥 Поездки не найдены...";

        public const string SearchType = "Введи город, страну или дату поездки в формате ДД.ММ.ГГГГ:";
        public const string SearchDate = "Введите дату предполагаемой поездки: ";
        public const string SearchCity = "Введите город, в который хотите поехать: ";

        public const string PostsFound = "Нашел новые посты";
        public const string PostsNotFound = "🤥 Посты не найдены...";
        public const string AllTripsAccepted = "Все новые поездки приняты!";
        public const string AllTripsDeclined = "Все новые поездки отклонены!";

        public const string UploadPhotoError = "Кажется, не удалось загрузить фотографию. Попробуй еще раз.";
        public const string EnterStartDateError = "Дата начала поездки не может быть меньше текущей!";
        public const string EnterEndDateError = "Дата окончания поездки не может быть раньшь даты начала поездки!";
        public const string EnterDateError = "Введите корректную дату.";
    }
}
