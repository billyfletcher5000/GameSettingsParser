using System.IO;
using GameSettingsParser.Model;
using Newtonsoft.Json;

namespace GameSettingsParser.Settings;

public class UserSettings
{
    public static UserSettings Instance { get; private set; } = new();

    public ParsingProfileModel ParsingProfile { get; set; } = new();
    
    public string SelectedImageModel { get; set; } = "";
    
    public string SelectedMarkupType { get; set; } = "";

    public static void Save(string path)
    {
        try
        {
            using (StreamWriter writer = File.CreateText(path))
            {
                JsonSerializer serializer = JsonSerializer.Create();
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
                JsonSerializer serializer = JsonSerializer.Create();
                var newSettings = serializer.Deserialize(reader, typeof(UserSettings)) as UserSettings;
                if(newSettings != null)
                    Instance = newSettings;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}