namespace GameSettingsParser.Utility
{
    public static class ChangeTracker
    {
        public enum ChangeType
        {
            Parsing
        }
        
        public static void NotifyChange(ChangeType changeType)
        {
            OnChangeNotified?.Invoke(changeType);
        }

        public static event Action<ChangeType>? OnChangeNotified;
    }
}