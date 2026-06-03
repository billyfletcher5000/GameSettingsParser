using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.General;
using GameSettingsParser.Model.Configuration.ImageAnalysis;
using GameSettingsParser.Services.Configuration;
using Newtonsoft.Json;

namespace GameSettingsParser.Settings;

public class UserSettings : IConfigurationSource
{
    public static UserSettings Instance { get; private set; } = new();
    
    public string LastParsingProfilePath { get; set; } = "";
    
    public string SelectedImageModel { get; set; } = "";
    
    public string SelectedMarkupType { get; set; } = "";

    public ObservableCollection<IConfigurationModel> Configurations { get; init; } = 
    [
        new GeneralConfigurationModel()
    ];

    public struct WindowSettings
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowState WindowState { get; set; }
    }

    public WindowSettings? MainWindowSettings { get; set; } = null;

    public void OnConfigurationChangesApplied()
    {
        Save(SettingsPathHelper.GetSettingsFilePath());
    }
    
    public static void Save(string path)
    {
        try
        {
            using (StreamWriter writer = File.CreateText(path))
            {
                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto
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
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.Auto
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