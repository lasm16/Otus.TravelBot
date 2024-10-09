using ConsoleBot.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.Bots
{
    public class AdminBot : IBot
    {
        private static string _greetingMessage = "Добрый день, админ. Что будем делать?";

        public string SendGreetingMessage() => _greetingMessage;

        public void PerfomAction(string message)
        {
            switch (message)
            {
                case "Новые посты": GetNewPosts(); break;
                case "Опубликованные посты": GetPublishedPosts(); break;
            }
        }

        private List<Post> GetPublishedPosts()
        {
            throw new NotImplementedException();
        }

        public List<Post> GetNewPosts()
        {
            throw new NotImplementedException();
        }
        public void AcceptPost() { }
        public void DeclinePost() { }
        public void PinPost(Guid id) { }

        public void PerfomAction(string action, params string[]? args)
        {
            throw new NotImplementedException();
        }
    }
}
