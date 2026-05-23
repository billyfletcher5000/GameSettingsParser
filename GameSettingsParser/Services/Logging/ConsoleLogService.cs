namespace GameSettingsParser.Services.Logging
{
    public class ConsoleLogService : ILogService
    {
        private const ConsoleColor DebugColor = ConsoleColor.Cyan;
        private const ConsoleColor LogColor = ConsoleColor.White;
        private const ConsoleColor WarningColor = ConsoleColor.Yellow;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;
        
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            Write($"DEBUG: {message}", DebugColor);
        }

        public void Log(string message)
        {
            Write($"LOG: {message}", LogColor);
        }

        public void Warning(string message)
        {
            Write($"WARNING: {message}", WarningColor);
        }

        public void Error(string message)
        {
            Write($"ERROR: {message}", ErrorColor);
            Console.Error.WriteLine(message);
        }
        
        private static void Write(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            if (color == oldColor)
            {
                Console.Write(text);
            }
            else
            {
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}