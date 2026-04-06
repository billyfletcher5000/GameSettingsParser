using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using GameSettingsParser.Utility;

namespace GameSettingsParser.Model
{
    public class ParsingProfileModel
    {
        public record struct MarkupInstance
        {
            public MarkupTypeModel Type { get; set; }
            public Rect Rect { get; set; }
        }

        public class ImageInstance
        {
            public ImageModel Image { get; set; }
            public ObservableCollection<MarkupInstance> MarkupInstances => [];

            public bool Equivalent(ImageInstance other)
            {
                return Image.Equals(other.Image) && MarkupInstances.SequenceEqual(other.MarkupInstances);
            }
        }

        public string Name { get; set; } = "Unsaved Profile";
        public List<ImageInstance> ImageInstances { get; } = [];
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
    }
}