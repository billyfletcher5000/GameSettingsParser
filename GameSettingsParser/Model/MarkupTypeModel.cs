using System.Windows.Media;
using GameSettingsParser.Settings.JsonConverters;
using Newtonsoft.Json;

namespace GameSettingsParser.Model;

public record struct MarkupTypeModel
{
    public string Name { get; set; } = String.Empty;
    
    [JsonConverter(typeof(ColorJsonConverter))]
    public Color Color { get; set; } = Colors.Blue;
    
    public bool IsDynamic { get; set; } = false;
    
    public string PositionedRelativeTo { get; set; } = String.Empty;
    
    public bool IsPositionedRelativeToOther => !string.IsNullOrEmpty(PositionedRelativeTo);

    public bool IsSearchArea { get; set; } = false;
    
    public string SearchArea { get; set; } = String.Empty;
    
    public bool HasSearchArea => !string.IsNullOrEmpty(SearchArea);
    
    public override string ToString() => Name;

    public MarkupTypeModel()
    {
    }
}