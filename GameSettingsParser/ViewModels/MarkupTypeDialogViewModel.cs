using System.Collections.ObjectModel;
using System.Windows.Media;
using GameSettingsParser.Model;

namespace GameSettingsParser.ViewModels
{
    public class MarkupTypeDialogViewModel : BindableBase
    {
        public sealed class ColorOption
        {
            public string Name { get; init; } = "";
            public Color Color { get; init; }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        public string TypeName
        {
            get => MarkupTypeModel.Name;
            set
            {
                MarkupTypeModel.Name = value;
                RaisePropertyChanged();
            }
        }
        
        private ColorOption _typeColor;
        public ColorOption TypeColor
        {
            get => _typeColor;
            set
            {
                _typeColor = value;
                MarkupTypeModel.Color = value.Color;
                RaisePropertyChanged();
            }
        }
        
        public bool IsDynamic
        {
            get => MarkupTypeModel.IsDynamic;
            set
            {
                if (MarkupTypeModel.IsDynamic != value)
                {
                    MarkupTypeModel.IsDynamic = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(HasSearchAreaOptions));
                }
            }
        }
        

        public bool IsSearchArea
        {
            get => MarkupTypeModel.IsSearchArea;
            set
            {
                MarkupTypeModel.IsSearchArea = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasSearchAreaOptions));
                RaisePropertyChanged(nameof(CanSetExportSignificance));
                RaisePropertyChanged(nameof(CanBeExportRowKey));
                RaisePropertyChanged(nameof(CanHaveExportPropertyOrder));
            }
        }

        public string PositionedRelativeTo
        {
            get => MarkupTypeModel.PositionedRelativeTo;
            set
            {
                MarkupTypeModel.PositionedRelativeTo = value;
                RaisePropertyChanged();
            }
        }

        public RelativePositioningType RelativePositioningType
        {
            get => MarkupTypeModel.RelativePositioningType;
            set
            {
                MarkupTypeModel.RelativePositioningType = value;
                RaisePropertyChanged();
            }
        }
        
        public IEnumerable<RelativePositioningType> RelativePositioningTypeValues => Enum.GetValues<RelativePositioningType>();

        public string SearchArea
        {
            get => MarkupTypeModel.SearchArea;
            set
            {
                MarkupTypeModel.SearchArea = value;
                RaisePropertyChanged();
            }
        }

        public ExportSignificance ExportSignificance
        {
            get => MarkupTypeModel.ExportSignificance;
            set
            {
                MarkupTypeModel.ExportSignificance = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanBeExportRowKey));
            }
        }
        
        public IEnumerable<ExportSignificance> ExportSignificanceValues => Enum.GetValues<ExportSignificance>();

        public bool IsExportRowKey
        {
            get => MarkupTypeModel.IsExportRowKey;
            set
            {
                MarkupTypeModel.IsExportRowKey = value;
                RaisePropertyChanged();
            }
        }
        
        public int ExportPropertyOrder
        {
            get => MarkupTypeModel.ExportPropertyOrder;
            set
            {
                MarkupTypeModel.ExportPropertyOrder = value;
                RaisePropertyChanged();
            }
        }
        
        public ObservableCollection<ColorOption> ColorOptions { get; }
        public ObservableCollection<string> PositionedRelativeToOptions { get; init; }
        public ObservableCollection<string> SearchAreaOptions { get; init; }
        
        public bool HasPositionedRelativeToOptions => PositionedRelativeToOptions.Count > 0;
        public bool HasSearchAreaOptions => SearchAreaOptions.Count > 0 && !IsSearchArea && IsDynamic;
        public bool IsPositionedRelativeToOther => !string.IsNullOrEmpty(PositionedRelativeTo);

        public bool CanSetExportSignificance => !IsSearchArea;
        public bool CanBeExportRowKey => ExportSignificance != ExportSignificance.Section && !IsSearchArea;
        public bool CanHaveExportPropertyOrder => ExportSignificance == ExportSignificance.ItemProperty && !IsSearchArea;

        public MarkupTypeModel MarkupTypeModel { get; }

        public MarkupTypeDialogViewModel(ParsingProfileModel parsingProfile)
        {
            _title = "New Markup Type";
            ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList(parsingProfile));
            _typeColor = ColorOptions.First();
            MarkupTypeModel = new MarkupTypeModel { Color = _typeColor.Color };
            PositionedRelativeToOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Select(type => type.Name));
            SearchAreaOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Where(type => type.IsSearchArea).Select(type => type.Name));
        }

        public MarkupTypeDialogViewModel(ParsingProfileModel parsingProfile, MarkupTypeModel markupType)
        {
            MarkupTypeModel = markupType;
            _title = "Edit Markup Type";
            ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList(parsingProfile, markupType));
            _typeColor = ColorOptions.First(option => option.Color == MarkupTypeModel.Color);
            var validOptions = parsingProfile.MarkupTypes.Where(type => !type.Name.Equals(markupType.Name) && !type.PositionedRelativeTo.Equals(markupType.Name)).Select(type => type.Name).ToList();
            SearchAreaOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Where(type => type.IsSearchArea && !type.Name.Equals(markupType.Name)).Select(type => type.Name));
            PositionedRelativeToOptions = new ObservableCollection<string>(validOptions);
        }
        
        private List<ColorOption> GetColorOptionList(ParsingProfileModel parsingProfile, MarkupTypeModel? selfMarkupType = null)
        {
            var type = typeof(Colors);
            var colorOptions = new List<ColorOption>();
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType == typeof(Color))
                {
                    var color = (Color)property.GetValue(null)!;
                    if (parsingProfile.MarkupTypes.Any(markupType =>
                        {
                            if (selfMarkupType != null && selfMarkupType.Color.Equals(color))
                                return false;
                            
                            return markupType.Color.Equals(color);
                        }))
                        continue;
                    
                    ColorOption colorOption = new ColorOption()
                    {
                        Name = property.Name,
                        Color = color
                    };
                    colorOptions.Add(colorOption);
                }
            }

            return colorOptions;
        }
    }
}