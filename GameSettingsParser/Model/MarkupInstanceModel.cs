using System.Windows;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class MarkupInstanceModel
    {
        [JsonProperty(IsReference = true)]
        public MarkupTypeModel? Type { get; set; }
        public Rect Rect { get; set; }
    }
}