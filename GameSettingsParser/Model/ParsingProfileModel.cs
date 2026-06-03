using System.Collections.ObjectModel;
using System.IO;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.ImageAnalysis;
using GameSettingsParser.Model.Configuration.Project;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Services.Configuration;
using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class ParsingProfileModel : IConfigurationSource
    {
        private bool _hasSelfChanges;

        [JsonIgnore]
        public string? FilePath { get; private set; }

        public string Name { get; set; } = "Untitled";
        public ObservableCollection<MarkupTypeModel> MarkupTypes { get; } = [];
        public ObservableCollection<ImageModel> Images { get; } = [];
        public ObservableCollection<ImageInstanceModel> ImageInstances { get; } = [];
        
        // TODO: There's got to be a more elegant way of handling default configuration models,
        //       possibly should be created on retrieval and have attribute/property to say
        //       which configuration source type it should be added to by default?
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public ObservableCollection<IConfigurationModel> Configurations { get; init; } = 
        [
            new ProjectConfigurationModel(),
            new TesseractImageAnalysisConfigurationModel(),
            new ColorSimilarityTextComparisonConfigurationModel(),
            new GoogleViTTextComparisonConfigurationModel(),
            new CombinedTextComparisonConfigurationModel()
        ];
        
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

        public void OnConfigurationChangesApplied()
        {
            HasChanges = true;
        }
        
        public void Save(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory '{path}' does not exist.");
            
            Save(this, path);
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
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto
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
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto,
                        ObjectCreationHandling = ObjectCreationHandling.Auto
                    });
                
                    if(serializer.Deserialize(reader, typeof(ParsingProfileModel)) is ParsingProfileModel loadedProfile)
                    {
                        loadedProfile.FilePath = path;
                        UpdateImagePathsWithProfilePath(loadedProfile, path);
                        return loadedProfile;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
        
        public static void UpdateImagePathsWithProfilePath(ParsingProfileModel profile, string profilePath)
        {
            var profileFolder = Path.GetDirectoryName(profilePath);
            if (profileFolder == null)
                return;
            
            foreach (var image in profile.Images)
            {
                if (!string.IsNullOrEmpty(image.RelativePath))
                    image.Path = Path.Combine(profileFolder, image.RelativePath);
            
                image.RelativePath = Path.GetRelativePath(profileFolder, image.Path);
            }
        }

        public static void ExportToPath(ParsingProfileModel profile, string path, string originalProfilePath)
        {
            UpdateImagePathsWithProfilePath(profile, originalProfilePath);
            
            var profileFolder = Path.GetDirectoryName(path);
            if (profileFolder == null)
                return;
            
            var imagesFolder = Path.Combine(profileFolder, "images");
            
            Directory.CreateDirectory(imagesFolder);
            foreach (var imageModel in profile.Images)
            {
                var imagePath = imageModel.Path;
                if (!File.Exists(imagePath))
                {
                    imagePath = Path.Combine(originalProfilePath, imageModel.RelativePath);
                    if (!File.Exists(imagePath))
                        continue;
                }

                var copiedFilename = Path.GetFileName(imageModel.RelativePath);
                var destinationPath = Path.Combine(imagesFolder, copiedFilename);
                File.Copy(imagePath, destinationPath);
                imageModel.RelativePath = Path.GetRelativePath(profileFolder, destinationPath);
            }
            
            Save(profile, path);
        }
    }
}