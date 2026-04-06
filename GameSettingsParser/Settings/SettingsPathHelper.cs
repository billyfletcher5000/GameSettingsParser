using System.Configuration;
using System.IO;

namespace GameSettingsParser.Settings
{
    public static class SettingsPathHelper
    {
        public static string GetSettingsFilePath()
        {
            var configuredPath = ConfigurationManager.AppSettings["UserSettingsPath"];

            if (!string.IsNullOrWhiteSpace(configuredPath))
                return configuredPath;

            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GameSettingsParser");

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "settings.json");
        }
    }
}