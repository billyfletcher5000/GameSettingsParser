using System.Windows.Media;

namespace GameSettingsParser.Model;

public record struct MarkupTypeModel
{
    public string Name { get; set; }
    public Color Color { get; set; }
    public bool IsDynamic { get; set; }
    
    public override string ToString() => Name;
}