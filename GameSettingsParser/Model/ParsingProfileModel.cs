using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class ParsingProfileModel
    {
        private bool _hasSelfChanges = false;
        private double _minimumDynamicComparisonConfidence = 0.0;
        private int _wordGapThreshold = 10;

        public string Name { get; set; } = "Untitled";
        public ObservableCollection<MarkupTypeModel> MarkupTypes { get; } = [];
        public ObservableCollection<ImageModel> Images { get; } = [];
        public ObservableCollection<ImageInstanceModel> ImageInstances { get; } = [];

        /// <summary>
        /// The amount of pixels between words' bounding boxes for them to be considered part of the same text string
        /// </summary>
        public int WordGapThreshold
        {
            get => _wordGapThreshold;
            set
            {
                if (_wordGapThreshold != value)
                {
                    _wordGapThreshold = value;
                    _hasSelfChanges = true;
                }
            }
        }

        public double MinimumDynamicComparisonConfidence
        {
            get => _minimumDynamicComparisonConfidence;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_minimumDynamicComparisonConfidence != value)
                {
                    _minimumDynamicComparisonConfidence = value;
                    _hasSelfChanges = true;
                }
            }
        }

        [JsonIgnore]
        public bool HasChanges 
        {
            get
            {
                if (_hasSelfChanges) 
                    return true;
                
                foreach (var imageModel in Images)
                {
                    if (imageModel.HasChanges)
                        return true;
                }

                foreach (var markupTypeModel in MarkupTypes)
                {
                    if (markupTypeModel.HasChanges)
                        return true;
                }

                foreach (var imageInstanceModel in ImageInstances)
                {
                    if (imageInstanceModel.HasChanges)
                        return true;
                }

                return false;
            }
            set
            {
                if (value)
                {
                    _hasSelfChanges = true;
                    ChangeTracker.NotifyChange(ChangeTracker.ChangeType.Parsing);
                }
                else
                {
                    _hasSelfChanges = false;
                    
                    foreach (var imageModel in Images)
                        imageModel.HasChanges = false;
                    
                    foreach (var markupTypeModel in MarkupTypes)
                        markupTypeModel.HasChanges = false;
                    
                    foreach (var imageInstanceModel in ImageInstances)
                        imageInstanceModel.HasChanges = false;
                }
            }
        } 

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
            
            foreach (var imageInstance in ImageInstances)
                imageInstance.MarkupInstances.RemoveAll(markupInstance => markupInstance.Type == markupType);
        }

        public MarkupTypeModel GetMarkupTypeByName(string name)
        {
            return MarkupTypes.First(type => type.Name == name);
        }

        public ParsingProfileModel()
        {
            Images.CollectionChanged += (_, _) => HasChanges = true;
            MarkupTypes.CollectionChanged += (_, _) => HasChanges = true;
            ImageInstances.CollectionChanged += (_, _) => HasChanges = true;
        }

        public static void Save(ParsingProfileModel profile, string path)
        {
            try
            {
                using (var writer = File.CreateText(path))
                {
                    var serializer = JsonSerializer.Create(new JsonSerializerSettings()
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        Formatting = Formatting.Indented
                    });
                    serializer.Serialize(writer, profile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static ParsingProfileModel? Load(string path)
        {
            try
            {
                if (!File.Exists(path)) 
                    return null;
            
                using (var reader = File.OpenText(path))
                {
                    var serializer = JsonSerializer.Create(new JsonSerializerSettings()
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        Formatting = Formatting.Indented
                    });
                
                    if(serializer.Deserialize(reader, typeof(ParsingProfileModel)) is ParsingProfileModel loadedProfile)
                        return loadedProfile;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
    }
}