namespace GameSettingsParser.Services.Logging
{
    public interface ILogService
    {
        public void Debug(string message);
        public void Log(string message);
        public void Warning(string message);
        public void Error(string message);
    }
}