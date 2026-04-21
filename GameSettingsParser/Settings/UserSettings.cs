using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GameSettingsParser.Model;
using GameSettingsParser.Model.TextComparisonConfiguration;
using GameSettingsParser.Services.TextComparison;
using Newtonsoft.Json;

namespace GameSettingsParser.Settings;

public class UserSettings
{
    public static UserSettings Instance { get; private set; } = new();
    
    public bool AutoOpenLastParsingProfile { get; set; } = true;
    
    public string LastParsingProfilePath { get; set; } = "";
    
    public string SelectedImageModel { get; set; } = "";
    
    public string SelectedMarkupType { get; set; } = "";
    
    public bool HighDebugVerbosity { get; set; } = false;
    
    public bool SaveAnalysisTemporaryImages { get; set; } = false;
    
    public ITextComparisonConfigurationModel DefaultTextComparisonConfiguration { get; set; } = new CombinedTextComparisonConfigurationModel();

    public struct WindowSettings
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowState WindowState { get; set; }
    }

    public WindowSettings? MainWindowSettings { get; set; } = null;
    
    public static void Save(string path)
    {
        try
        {
            using (StreamWriter writer = File.CreateText(path))
            {
                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    Formatting = Formatting.Indented
                });
                serializer.Serialize(writer, Instance);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }

    public static void Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            
            using (StreamReader reader = File.OpenText(path))
            {
                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                
                if(serializer.Deserialize(reader, typeof(UserSettings)) is UserSettings newSettings)
                    Instance = newSettings;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}