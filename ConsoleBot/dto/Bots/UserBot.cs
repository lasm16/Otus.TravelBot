using ConsoleBot.dto.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleBot.dto.Bots
{
    public class UserBot : IBot
    {
        private User _currentUser;
        private List<User> _users;

        public UserBot(User user)
        {
            _currentUser = user;
        }

        public void SendGreetingMessage() => Console.WriteLine($"Приветствую тебя, {_currentUser.Name}! Ты можешь выложить пост о планируемой поездке или найти попутчика");

        public void CreateNewPost()
        {
            Console.WriteLine("Для того чтобы ваш пост был размещён в группе он должен соответствовать следующему описанию");

            Console.WriteLine("1) Предполагаемую дату начала поездки в формате ДД.ММ.ГГГГ: ");
            DateTime.TryParse(Console.ReadLine(), out DateTime dateTimeStart);

            Console.WriteLine("2) Предполагаемую дату окончания поездки в формате ДД.ММ.ГГГГ: ");
            DateTime.TryParse(Console.ReadLine(), out DateTime dateTimeEnd);

            Console.WriteLine("3) краткое описание плана по вашему путешествию.");
            var description = Console.ReadLine();
            Console.WriteLine("4) ваше фото.");
            Console.WriteLine("5) реальный аккаунт ВК (посты от фейк аккаунтов и закрытыми профилями не публикуем. Нужно только для проверки админу)");
            var link = Console.ReadLine();
            var post = new Post(new Guid(), dateTimeStart, dateTimeEnd, description, link);
            Console.WriteLine("Почти все готово! После проверки сообщения админстрацией ваш пост будет опубликован!");
        }
        public List<Post> FindAllPosts()
        {
            Console.WriteLine("Нашел твои поездки:");
            return _currentUser.Posts;
        }
        public IEnumerable<Post?>? SearchFellow(DateTime dateTime)
        {
            _users.Remove(_currentUser);

            //var users = _users.Find(x => x.Posts.Where(p => p.TravelDateStart == dateTime));
            var posts = _users.Select(x => x.Posts.Find(p => p.TravelDateStart == dateTime));
            if (posts == null)
            {
                Console.WriteLine("Поездок с такой датой не найдено!");
            }
            return posts;
        }

        public void UpdatePost(Guid id)
        {
            var post = _currentUser.Posts.FirstOrDefault(x => x.Id == id);
            if (post != null)
            {
                Console.WriteLine("Для того чтобы ваш пост был размещён в группе он должен соответствовать следующему описанию");

                Console.WriteLine("1) Предполагаемую дату начала поездки в формате ДД.ММ.ГГГГ: ");
                DateTime.TryParse(Console.ReadLine(), out DateTime dateTimeStart);

                Console.WriteLine("2) Предполагаемую дату окончания поездки в формате ДД.ММ.ГГГГ: ");
                DateTime.TryParse(Console.ReadLine(), out DateTime dateTimeEnd);

                Console.WriteLine("3) краткое описание плана по вашему путешествию.");
                var description = Console.ReadLine();
                Console.WriteLine("4) ваше фото.");
                Console.WriteLine("5) реальный аккаунт ВК (посты от фейк аккаунтов и закрытыми профилями не публикуем. Нужно только для проверки админу)");
                var link = Console.ReadLine();
                post.TravelDateStart = dateTimeStart;
                post.TravelDateEnd = dateTimeEnd;
                post.Description = description;
                post.Link = link;
                post.Status = "Проверяется";
                Console.WriteLine("Почти все готово! После проверки сообщения админстрацией ваш пост будет опубликован!");
            }
            Console.WriteLine($"Пост с id = {id} не найден!");
        }



        public void DeletePost(Guid id)
        {
            var post = _currentUser.Posts.FirstOrDefault(x => x.Id == id);
            if (post != null)
            {
                _currentUser.Posts.Remove(post);
            }
            Console.WriteLine($"Пост с id = {id} не найден!");
        }

        public void MakePostAsVip(Guid id)
        {
            var post = _currentUser.Posts.FirstOrDefault(x => x.Id == id);
            if (post != null)
            {
                post.IsVipRequested = true;
            }
            Console.WriteLine($"Пост с id = {id} не найден!");
        }
    }
}
