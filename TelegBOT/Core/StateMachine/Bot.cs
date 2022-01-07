using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegBOT.Core.StateMachine
{
    public class Bot : IBot
    {
        public BotState State { get; set; }
        public long ChatID { get ; set ; }

        public Bot(long chatID, BotState bs)
        {
            ChatID = chatID;
            State = bs;
        }

        public void SetState(BotState botState)
        {
            Console.WriteLine($"The state for chat {ChatID} has changed: " + botState) ;

            State = botState;
        }
    }
}
