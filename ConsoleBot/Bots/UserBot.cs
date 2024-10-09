using ConsoleBot.dto;
using ConsoleBot.dto.Users;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleBot.Bots
{
    public class UserBot : IBot
    {
        private User _currentUser;
        private List<User> _users;

        public List<Post> Posts
        {
            get
            {
                //var ss = _users.Select(x => x.Posts);
                return null;
            }
        }

        public UserBot(User user)
        {
            _currentUser = user;
        }

        public string SendGreetingMessage() => $"Приветствую тебя, {_currentUser.Name}! Ты можешь выложить пост о планируемой поездке или найти попутчика";

        private void CreateNewPost(string[] args)
        {
            DateTime.TryParse(args[0], out DateTime dateTimeStart);
            DateTime.TryParse(args[1], out DateTime dateTimeEnd);
            var description = args[2];
            var link = args[3];
            var post = new Post(new Guid(), dateTimeStart, dateTimeEnd, description, link);
            _currentUser.Posts.Add(post);
        }
        private void FindAllPosts()
        {
            var posts = _currentUser.Posts; // не нравится. Нужно возвращать посты
            foreach (var post in posts)
            {
                Console.WriteLine(post);
            }
        }

        private List<Post?>? SearchFellow(string[] args)
        {
            DateTime.TryParse(args[0], out DateTime dateTime);
            _users.Remove(_currentUser);

            var posts = _users.Select(x => x.Posts.Find(p => p.TravelDateStart == dateTime)).ToList();
            if (posts == null)
            {
                Console.WriteLine($"Поездок с датой = {dateTime} не найдено!");
            }
            return posts;
        }

        private void UpdatePost(string[] args) // не нравится
        {
            Guid.TryParse(args[0], out var guid);
            var post = _currentUser.Posts.FirstOrDefault(x => x.Id == guid);
            DateTime.TryParse(args[1], out var dateTimeStart);
            DateTime.TryParse(args[2], out var dateTimeEnd);
            var description = args[3];
            var link = args[3];

            if (post != null)
            {
                post.TravelDateStart = dateTimeStart;
                post.TravelDateEnd = dateTimeEnd;
                post.Description = description;
                post.LinkToVk = link;
                post.Status = "Проверяется";
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }

        public void DeletePost(string[] args)
        {
            Guid.TryParse(args[0], out var guid);
            var post = _currentUser.Posts.FirstOrDefault(x => x.Id == guid);
            if (post != null)
            {
                _currentUser.Posts.Remove(post);
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }

        public void MakePostAsVip(string[] args)
        {
            Guid.TryParse(args[0], out var guid);
            var post = _currentUser.Posts.FirstOrDefault(x => x.Id == guid);
            if (post != null)
            {
                post.IsVipRequested = true;
            }
            Console.WriteLine($"Пост с id = {guid} не найден!");
        }

        public void PerfomAction(string message, params string[] args)
        {
            switch (message) // желательно не использовать switch-case
            {
                case "Создать новую поездку": CreateNewPost(args); break;
                case "Найти попутчика": SearchFellow(args); break;
                case "Мои поездки": FindAllPosts(); break;
                case "Удалить пост": DeletePost(args); break;
                case "Редактировать пост": UpdatePost(args); break;
                case "Сделать VIP-пост": MakePostAsVip(args); break;
            }
        }
    }
}
