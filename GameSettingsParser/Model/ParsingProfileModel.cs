using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using GameSettingsParser.Utility;

namespace GameSettingsParser.Model
{
    public class ParsingProfileModel
    {
        public string Name { get; set; } = "Unsaved Profile";
        public List<ImageInstanceModel> ImageInstances { get; } = [];
        public ObservableCollection<MarkupTypeModel> MarkupTypes { get; } = [];
        public ObservableCollection<ImageModel> Images { get; } = [];

        public bool IsImageModelInUse(ImageModel image)
        {
            return ImageInstances.Any(instance => instance.Image == image && instance.MarkupInstances.Count > 0);
        }

        public void RemoveImageModel(ImageModel image)
        {
            Images.Remove(image);
            ImageInstances.RemoveAll(instance => instance.Image == image);
        }

        public bool IsMarkupTypeInUse(MarkupTypeModel markupType)
        {
            return ImageInstances.Any(imageInstance =>
                imageInstance.MarkupInstances.Any(markupInstance => markupInstance.Type == markupType));
        }

        public void RemoveMarkupType(MarkupTypeModel markupType)
        {
            MarkupTypes.Remove(markupType);
            ImageInstances.ForEach(imageInstance => imageInstance.MarkupInstances.RemoveAll(markupInstance => markupInstance.Type == markupType));
        }

        public MarkupTypeModel GetMarkupTypeByName(string name)
        {
            return MarkupTypes.First(type => type.Name == name);
        }
    }
}