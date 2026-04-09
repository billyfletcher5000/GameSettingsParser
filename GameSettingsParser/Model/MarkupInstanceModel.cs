using System.Windows;

namespace GameSettingsParser.Model
{
    public record struct MarkupInstanceModel
    {
        public MarkupTypeModel Type { get; set; }
        public Rect Rect { get; set; }
    }
}