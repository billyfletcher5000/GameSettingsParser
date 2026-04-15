using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class ImageInstanceModel
    {
        [JsonProperty(IsReference = true)]
        public ImageModel? Image { get; set; } = null;
        
        public ObservableCollection<MarkupInstanceModel> MarkupInstances { get; init; } = [];
    }
}