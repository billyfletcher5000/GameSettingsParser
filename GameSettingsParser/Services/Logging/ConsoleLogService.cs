namespace GameSettingsParser.Services.Logging
{
    public class ConsoleLogService : ILogService
    {
        public enum LogLevel
        {
            Debug,
            Log,
            Warning,
            Error
        }

        public static LogLevel Level = LogLevel.Debug;
        
        private const string DebugColor = "\u001b[32m";
        private const string LogColor = ResetColor;
        private const string WarningColor = "\u001b[33m";
        private const string ErrorColor = "\u001b[31m";
        private const string ResetColor = "\u001b[0m";
        
        public void Debug(string message)
        {
            if (Level > LogLevel.Debug)
                return;
            
            System.Diagnostics.Debug.WriteLine(message);
            Write($"DEBUG: {message}", DebugColor);
        }

        public void Log(string message)
        {
            if (Level > LogLevel.Log)
                return;
            
            Write($"LOG: {message}", LogColor);
        }

        public void Warning(string message)
        {
            if(Level > LogLevel.Warning)
                return;
            
            Write($"WARNING: {message}", WarningColor);
        }

        public void Error(string message)
        {
            if (Level > LogLevel.Error)
                return;
            
            Write($"ERROR: {message}", ErrorColor);
            Console.Error.WriteLine(message);
        }
        
        private static void Write(string text, string color)
        {
            Console.WriteLine($"{color}{text}{ResetColor}");
        }
    }
}