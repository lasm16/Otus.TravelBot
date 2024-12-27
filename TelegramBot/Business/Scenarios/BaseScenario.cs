using Common.Model;
using Common.Model.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Business.Bot.Roles;
using TelegramBot.Business.Scenarios.AdminScenarios;
using TelegramBot.Business.Scenarios.UserScenarios;

namespace TelegramBot.Business.Scenarios
{
    public abstract class BaseScenario(TelegramBotClient botClient)
    {
        public IRole Role { get; set; }
        public Common.Model.User? User { get; set; }
        public TelegramBotClient BotClient { get; set; } = botClient;

        private readonly string? _adminName = AppConfig.AdminName;

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
            User = GetUser(tgUser!); // здесь должна быть проверка роли админ/юзер через значение в бд
            if (UserType.Admin == User.UserType)
            {
                SetAdminActions();
            }
            else if (UserType.SimpleUser == User.UserType)
            {
                SetUserActions();
            }
        }

        private Common.Model.User GetUser(Telegram.Bot.Types.User user)
        {
            var nickName = user.Username;
            if (nickName == _adminName)
            {
                return new Common.Model.User()
                {
                    Id = user.Id,
                    UserName = user.Username,
                    UserType = UserType.Admin
                };
            }
            return new Common.Model.User()
            {
                Id = user.Id,
                UserName = user.Username,
                UserType = UserType.SimpleUser
            };
        }

        private void SetUserActions()
        {
            Role = new UserRole
            {
                Actions = new Dictionary<string, IScenario>
                {
                    { "Новая поездка",      new CreateNewTripScenario(BotClient, User) },
                    { "Мои поездки",        new ShowMyTripsScenario(BotClient, User) },
                    { "Найти попутчика",    new FindFellowScenario(BotClient, User) }
                }
            };
        }

        private void SetAdminActions()
        {
            Role = new AdminRole
            {
                Actions = new Dictionary<string, IScenario>
                {
                    { "Новые посты",        new ShowNewPostsScenario(BotClient) },
                    { "Новая поездка",      new CreateNewTripScenario(BotClient, User) },
                    { "Мои поездки",        new ShowMyTripsScenario(BotClient, User) },
                    { "Найти попутчика",    new FindFellowScenario(BotClient, User) }
                }
            };
        }
    }
}
