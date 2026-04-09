using System.Collections.ObjectModel;

namespace GameSettingsParser.Model
{
    public class ImageInstanceModel
    {
        public ImageModel Image { get; set; }
        public ObservableCollection<MarkupInstanceModel> MarkupInstances { get; init; } = [];

        public bool Equivalent(ImageInstanceModel other)
        {
            return Image.Equals(other.Image) && MarkupInstances.SequenceEqual(other.MarkupInstances);
        }
    }
}