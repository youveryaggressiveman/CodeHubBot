namespace TelegBOT.Core.StateMachine
{
    public interface IBot
    {
        long ChatID { get; set; }

        BotState State { get; set; }

        void SetState(BotState botState);
    }
}