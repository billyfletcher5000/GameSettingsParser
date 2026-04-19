using System.Windows.Media;
using GameSettingsParser.Settings.JsonConverters;
using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model
{
    public enum ExportSignificance
    {
        Section,
        ItemProperty
    }
    
    public enum RelativePositioningType
    {
        TopLeft,
        MiddleLeft,
        BottomLeft,
        TopMiddle,
        MiddleMiddle,
        BottomMiddle,
        TopRight,
        MiddleRight,
        BottomRight,
    }

    public class MarkupTypeModel
    {
        private string _name = String.Empty;
        private Color _color = Colors.Blue;
        private bool _isDynamic = false;
        private string _positionedRelativeTo = String.Empty;
        private RelativePositioningType _relativePositioningType = RelativePositioningType.MiddleLeft;
        private bool _isSearchArea = false;
        private string _searchArea = String.Empty;
        private ExportSignificance _exportSignificance = ExportSignificance.ItemProperty;
        private int _exportPropertyOrder = 0;
        private bool _isExportRowKey = false;
        private string _htmlExportTableWidth = string.Empty;
        private bool _hasChanges = false;

        public string Name
        {
            get => _name;
            set
            {
                if (!string.Equals(_name, value))
                {
                    _name = value;
                    HasChanges = true;
                }
            }
        }

        [JsonConverter(typeof(ColorJsonConverter))]
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    HasChanges = true;
                }
            }
        }

        public bool IsDynamic
        {
            get => _isDynamic;
            set
            {
                if (_isDynamic != value)
                {
                    _isDynamic = value;
                    HasChanges = true;
                }
            }
        }

        public string PositionedRelativeTo
        {
            get => _positionedRelativeTo;
            set
            {
                if (!string.Equals(_positionedRelativeTo, value))
                {
                    _positionedRelativeTo = value;
                    HasChanges = true;
                }
            }
        }

        public bool IsPositionedRelativeToOther => !string.IsNullOrEmpty(PositionedRelativeTo);

        public RelativePositioningType RelativePositioningType
        {
            get => _relativePositioningType;
            set
            {
                if (_relativePositioningType != value)
                {
                    _relativePositioningType = value;
                    HasChanges = true;
                }
            }
        }

        public bool IsSearchArea
        {
            get => _isSearchArea;
            set
            {
                if (_isSearchArea != value)
                {
                    _isSearchArea = value;
                    HasChanges = true;
                }
            }
        }

        public string SearchArea
        {
            get => _searchArea;
            set
            {
                if (!string.Equals(_searchArea, value))
                {
                    _searchArea = value;
                    HasChanges = true;
                }
            }
        }

        public bool HasSearchArea => !string.IsNullOrEmpty(SearchArea);

        public ExportSignificance ExportSignificance
        {
            get => _exportSignificance;
            set
            {
                if (_exportSignificance != value)
                {
                    _exportSignificance = value;
                    HasChanges = true;
                }
            }
        }

        public int ExportPropertyOrder
        {
            get => _exportPropertyOrder;
            set
            {
                if (_exportPropertyOrder != value)
                {
                    _exportPropertyOrder = value;
                    HasChanges = true;
                }
            } 
        }

        public bool IsExportRowKey
        {
            get => _isExportRowKey;
            set
            {
                if (_isExportRowKey != value)
                {
                    _isExportRowKey = value;
                    HasChanges = true;
                }
            }
        }

        public string HTMLExportTableWidth
        {
            get => _htmlExportTableWidth;
            set
            {
                if (!string.Equals(_htmlExportTableWidth, value))
                {
                    _htmlExportTableWidth = value;
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

        public override string ToString() => Name;
    }
}