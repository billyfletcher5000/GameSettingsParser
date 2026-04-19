using System.Collections.ObjectModel;
using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class ImageInstanceModel
    {
        private ImageModel? _image = null;
        private bool _hasChanges = false;

        [JsonProperty(IsReference = true)]
        public ImageModel? Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    HasChanges = true;
                }
            } 
        }

        public ObservableCollection<MarkupInstanceModel> MarkupInstances { get; init; } = [];

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (value)
                {
                    _hasChanges = true;
                    ChangeTracker.NotifyChange(ChangeTracker.ChangeType.Parsing);
                }
                else
                {
                    _hasChanges = false;
                    foreach (var markupInstance in MarkupInstances)
                        markupInstance.HasChanges = false;
                }
            }
        }

        public ImageInstanceModel()
        {
            MarkupInstances.CollectionChanged += (_, _) => HasChanges = true;
        }
    }
}