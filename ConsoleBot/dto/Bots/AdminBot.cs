using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.dto.Bots
{
    public class AdminBot : IBot
    {
        public void AcceptPost() { }
        public void DeclinePost() { }
        public void PinPost(Guid id) { }
    }
}
