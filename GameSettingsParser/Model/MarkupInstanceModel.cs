using System.Windows;
using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public class MarkupInstanceModel
    {
        private Rect _rect;
        private MarkupTypeModel? _type;
        private bool _hasChanges = false;

        [JsonProperty(IsReference = true)]
        public MarkupTypeModel? Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    HasChanges = true;
                }
            }
        }

        public Rect Rect
        {
            get => _rect;
            set
            {
                if (_rect != value)
                {
                    _rect = value;
                    HasChanges = true;
                }
            }
        }

        [JsonIgnore]
        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                if(value)
                    ChangeTracker.NotifyChange(ChangeTracker.ChangeType.Parsing);
            }
        }
    }
}