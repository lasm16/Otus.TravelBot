using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.service
{
    public interface IBotService
    {
        public void Greeting();
        public List<string> Actions { get; }
        public void LauchScenario(string action);
    }
}
