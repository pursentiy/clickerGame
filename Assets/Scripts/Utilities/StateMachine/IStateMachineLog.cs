namespace Platform.Common.Utilities.StateMachine
{
    public interface IStateMachineLog
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}