using ConsoleBot.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.Bots
{
    public interface IBot
    {
        public string SendGreetingMessage();
        public void PerfomAction(string action, params string[] args); // не нравится
    }
}
