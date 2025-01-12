using Common.Model.Bot;
using DataBase;
using DataBase.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Business.Bot.Roles;
using TelegramBot.Business.Scenarios.AdminScenarios;
using TelegramBot.Business.Scenarios.UserScenarios;

namespace TelegramBot.Business.Scenarios
{
    public abstract class BaseScenario(TelegramBotClient botClient)
    {
        public IRole? Role { get; set; }
        public DataBase.Models.User? User { get; set; }
        public TelegramBotClient BotClient { get; set; } = botClient;
        private static List<long> _admins => GetAdminIds();

        public IScenario GetScenario(Update update)
        {
            var action = update.CallbackQuery!.Data;
            // кидает NRE после нажатия кнопки "Готово" у подтверждения поездки
            return Role!.Actions!.FirstOrDefault(t => t.Key == action).Value;
        }

        public void CheckRole(Message message)
        {
            if (Role != null)
            {
                return;
            }
            var tgUser = message.From;
            User = GetUser(tgUser!);
            if (UserType.Admin == User.Type)
            {
                SetAdminActions();
            }
            else if (UserType.SimpleUser == User.Type)
            {
                SetUserActions();
            }
        }

        private DataBase.Models.User GetUser(Telegram.Bot.Types.User user)
        {
            var userId = user.Id;
            if (_admins.Contains(userId))
            {
                return new DataBase.Models.User()
                {
                    Id = user.Id,
                    NickName = user.Username,
                    Type = UserType.Admin
                };
            }
            return new DataBase.Models.User()
            {
                Id = user.Id,
                NickName = user.Username,
                Type = UserType.SimpleUser
            };
        }

        private void SetUserActions()
        {
            Role = new UserRole
            {
                Actions = new Dictionary<string, IScenario>
                {
                    { "Новая поездка",      new CreateNewTripScenario(BotClient, User!) },
                    { "Мои поездки",        new ShowMyTripsScenario(BotClient, User!) },
                    { "Найти попутчика",    new FindFellowScenario(BotClient, User!) }
                }
            };
        }

        private void SetAdminActions()
        {
            Role = new AdminRole
            {
                Actions = new Dictionary<string, IScenario>
                {
                    { "Новые посты",        new ShowNewPostsScenario(BotClient, User!) },
                    { "Новая поездка",      new CreateNewTripScenario(BotClient, User!) },
                    { "Мои поездки",        new ShowMyTripsScenario(BotClient, User!) },
                    { "Найти попутчика",    new FindFellowScenario(BotClient, User!) }
                }
            };
        }

        private static List<long> GetAdminIds()
        {
            using var db = new ApplicationContext();
            var users = db.Users.ToList();
            return users.Where(x => x.Type == UserType.Admin).Select(x => x.Id).ToList();
        }
    }
}
