namespace TelegBOT.Core.StateMachine
{
    public enum BotState
    {
        HANDLE_COMMAND,
        HANDLE_REGISTER_ANSWER,
        HANDLE_REGISTER_GROUP, 
        HANDLE_REGISTER_COURSE,
        HANDLE_REGISTER_CONFIRM,
        HANDLE_CHANGE_FIO,
    }
}
