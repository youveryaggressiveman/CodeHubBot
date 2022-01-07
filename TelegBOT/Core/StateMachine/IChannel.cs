namespace TelegBOT.Core.StateMachine
{
    public interface IChannel
    {
        long ChatID { get; set; }

        BotState State { get; set; }

        void SetState(BotState botState);
    }
}