using System;

namespace TelegBOT.Core.StateMachine
{
    public class ChatChannel : IChannel
    {
        public BotState State { get; set; }
        public long ChatID { get ; set ; }

        public ChatChannel(long chatID, BotState bs)
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
