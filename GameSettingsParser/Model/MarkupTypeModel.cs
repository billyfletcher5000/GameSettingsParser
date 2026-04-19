using System.Windows.Media;
using GameSettingsParser.Settings.JsonConverters;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public enum ExportSignificance
    {
        Section,
        ItemProperty
    }
    
    public enum RelativePositioningType
    {
        TopLeft,
        MiddleLeft,
        BottomLeft,
        TopMiddle,
        MiddleMiddle,
        BottomMiddle,
        TopRight,
        MiddleRight,
        BottomRight,
    }

    public class MarkupTypeModel
    {
        public string Name { get; set; } = String.Empty;
    
        [JsonConverter(typeof(ColorJsonConverter))]
        public Color Color { get; set; } = Colors.Blue;
    
        public bool IsDynamic { get; set; } = false;
    
        public string PositionedRelativeTo { get; set; } = String.Empty;
    
        public bool IsPositionedRelativeToOther => !string.IsNullOrEmpty(PositionedRelativeTo);
        
        public RelativePositioningType RelativePositioningType { get; set; } = RelativePositioningType.MiddleLeft;

        public bool IsSearchArea { get; set; } = false;
    
        public string SearchArea { get; set; } = String.Empty;
    
        public bool HasSearchArea => !string.IsNullOrEmpty(SearchArea);
        
        public ExportSignificance ExportSignificance { get; set; } = ExportSignificance.ItemProperty;
        
        public int ExportPropertyOrder { get; set; } = 0;
        
        public bool IsExportRowKey { get; set; } = false;
        
        public string HTMLExportTableWidth { get; set; } = string.Empty;
    
        public override string ToString() => Name;

        public MarkupTypeModel()
        {
        }
    }
}