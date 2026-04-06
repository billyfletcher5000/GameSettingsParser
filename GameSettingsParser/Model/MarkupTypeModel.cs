using System.Windows.Media;
using GameSettingsParser.Settings.JsonConverters;
using Newtonsoft.Json;

namespace GameSettingsParser.Model;

public record struct MarkupTypeModel
{
    public string Name { get; set; }
    
    [JsonConverter(typeof(ColorJsonConverter))]
    public Color Color { get; set; }
    
    public bool IsDynamic { get; set; }
    
    public override string ToString() => Name;
}