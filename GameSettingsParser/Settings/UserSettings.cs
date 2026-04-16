using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GameSettingsParser.Model;
using GameSettingsParser.Services.TextComparison;
using Newtonsoft.Json;

namespace GameSettingsParser.Settings;

public class UserSettings
{
    public static UserSettings Instance { get; private set; } = new();

    public ParsingProfileModel ParsingProfile { get; set; } = new();
    
    public string SelectedImageModel { get; set; } = "";
    
    public string SelectedMarkupType { get; set; } = "";
    
    /// <summary>
    /// The amount of pixels between words' bounding boxes for them to be considered part of the same text string
    /// </summary>
    public int WordGapThreshold { get; set; } = 10;

    public float MinimumDynamicComparisonConfidence { get; set; } = 0.0f;
    
    public ObservableCollection<Type> TextComparisonServices { get; init; } = [typeof(ColorSimilarityTextComparisonService)];

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
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
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