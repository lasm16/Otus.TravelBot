using ConsoleBot.Bots;
using ConsoleBot.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.service
{
    public class ConsoleBotService : IBotService
    {
        private IBot _bot;
        private List<string> _userActionList = ["Создать новую поездку", "Найти попутчика", "Мои поездки", "Редактировать пост", "Удалить пост", "Запрос на VIP-пост"];
        private List<string> _adminActionList = ["Новые посты", "Опубликованные посты", "Принять", "Отклонить", "Сделать VIP-пост"];

        public List<string> Actions
        {
            get
            {
                if (_bot is UserBot) // нужен нормальный DI
                {
                    return _userActionList;
                }
                return _adminActionList;
            }
        }

        public ConsoleBotService(IBot bot)
        {
            _bot = bot;
        }

        public void Greeting()
        {
            var message = _bot.SendGreetingMessage();
            SendMessage(message);
        }

        public void LauchScenario(string action)
        {
            switch (action)
            {
                case "Создать новую поездку": CreateNewTravelScenario(); break;
                case "Найти попутчика": FindFellowScenario(); break;
                case "Мои поездки": ShowMyTravelScenario(); break;
                case "Редактировать пост": UpdatePostScenario(); break;
                case "Удалить пост": DeletePostScenario(); break;
                case "Запрос на VIP-пост": MakeVipPostScenario(); break;
                default: SendMessage(BotPhrases.UnavailableActions); break;
            }
        }

        private void MakeVipPostScenario()
        {
            SendMessage(BotPhrases.PostForVip);
            var guid = GetLineFromConsole();
            var action = _userActionList[5];
            _bot.PerfomAction(action, guid);
        }

        private void DeletePostScenario()
        {
            SendMessage(BotPhrases.PostForDelete);
            var guid = GetLineFromConsole();
            var action = _userActionList[4];
            _bot.PerfomAction(action, guid);
        }

        private void UpdatePostScenario()
        {
            SendMessage(BotPhrases.PostForUpdate);
            var guid = GetLineFromConsole();
            var array = GetInitialData();
            var action = _userActionList[3];
            _bot.PerfomAction(action, guid, array[0], array[1], array[2], array[3]); // не нравится
        }

        private void FindFellowScenario()
        {
            SendMessage(BotPhrases.SuggestDate);
            var dateTimeStart = GetLineFromConsole();
            var action = _userActionList[1];
            _bot.PerfomAction(action, dateTimeStart);
            SendMessage(BotPhrases.Done);
        }

        private void ShowMyTravelScenario()
        {
            SendMessage(BotPhrases.FindTrips);
            var action = _userActionList[2];
            _bot.PerfomAction(action);
        }

        private void CreateNewTravelScenario()
        {
            var array = GetInitialData();
            var action = _userActionList[0];
            _bot.PerfomAction(action, array[0], array[1], array[2], array[3]); // не нравится
            SendMessage(BotPhrases.Done);
        }

        private string[] GetInitialData() // не нравится
        {
            SendMessage(BotPhrases.Agreement);
            SendMessage(BotPhrases.SuggestStartDate);
            var dateTimeStart = GetLineFromConsole();
            SendMessage(BotPhrases.SuggestEndDate);
            var dateTimeEnd = GetLineFromConsole();
            SendMessage(BotPhrases.Description);
            var description = GetLineFromConsole();
            SendMessage(BotPhrases.Photo);
            SendMessage(BotPhrases.LinkVk);
            var link = GetLineFromConsole();

            return [dateTimeStart, dateTimeEnd, description, link];
        }

        private void SendMessage(string text) => Console.WriteLine(text);
        private string GetLineFromConsole() => Console.ReadLine();
    }
}
