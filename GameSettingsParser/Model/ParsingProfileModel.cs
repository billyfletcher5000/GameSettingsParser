using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class ParsingProfileModel
    {
        public string Name { get; set; } = "Unsaved Profile";
        public ObservableCollection<MarkupTypeModel> MarkupTypes { get; } = [];
        public ObservableCollection<ImageModel> Images { get; } = [];
        public List<ImageInstanceModel> ImageInstances { get; } = [];
    
        /// <summary>
        /// The amount of pixels between words' bounding boxes for them to be considered part of the same text string
        /// </summary>
        public int WordGapThreshold { get; set; } = 10;

        public double MinimumDynamicComparisonConfidence { get; set; } = 0.0;

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