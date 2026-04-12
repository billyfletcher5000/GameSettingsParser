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
            get => _markupType.Name;
            set
            {
                _markupType.Name = value;
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
                _markupType.Color = value.Color;
                RaisePropertyChanged();
            }
        }
        
        public bool IsDynamic
        {
            get => _markupType.IsDynamic;
            set
            {
                if (_markupType.IsDynamic != value)
                {
                    _markupType.IsDynamic = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(HasSearchAreaOptions));
                    RaisePropertyChanged(nameof(IsMultilineSelectionAvailable));
                }
            }
        }

        public bool IsMultiline
        {
            get => _markupType.IsMultiline;
            set
            {
                _markupType.IsMultiline = value;
                RaisePropertyChanged();
            }
        }

        public bool IsMultilineSelectionAvailable => _markupType.IsDynamic == false;

        public bool IsSearchArea
        {
            get => _markupType.IsSearchArea;
            set
            {
                _markupType.IsSearchArea = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasSearchAreaOptions));
            }
        }

        public string PositionedRelativeTo
        {
            get => _markupType.PositionedRelativeTo;
            set
            {
                _markupType.PositionedRelativeTo = value;
                RaisePropertyChanged();
            }
        }

        public string SearchArea
        {
            get => _markupType.SearchArea;
            set
            {
                _markupType.SearchArea = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ColorOption> ColorOptions { get; }
        public ObservableCollection<string> PositionedRelativeToOptions { get; init; }
        public ObservableCollection<string> SearchAreaOptions { get; init; }
        
        public bool HasPositionedRelativeToOptions => PositionedRelativeToOptions.Count > 0;
        public bool HasSearchAreaOptions => SearchAreaOptions.Count > 0 && !IsSearchArea && IsDynamic;

        private MarkupTypeModel _markupType;
        public MarkupTypeModel MarkupTypeModel => _markupType;

        public MarkupTypeDialogViewModel(ParsingProfileModel parsingProfile)
        {
            _title = "New Markup Type";
            ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList(parsingProfile));
            _typeColor = ColorOptions.First();
            _markupType = new() { Color = _typeColor.Color };
            PositionedRelativeToOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Select(type => type.Name));
            SearchAreaOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Where(type => type.IsSearchArea).Select(type => type.Name));
        }

        public MarkupTypeDialogViewModel(ParsingProfileModel parsingProfile, MarkupTypeModel markupType)
        {
            _markupType = markupType;
            _title = "Edit Markup Type";
            ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList(parsingProfile));
            _typeColor = ColorOptions.First(option => option.Color == _markupType.Color);
            var validOptions = parsingProfile.MarkupTypes.Where(type => !type.Name.Equals(markupType.Name) && !type.PositionedRelativeTo.Equals(markupType.Name)).Select(type => type.Name).ToList();
            SearchAreaOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Where(type => type.IsSearchArea && !type.Name.Equals(markupType.Name)).Select(type => type.Name));
            PositionedRelativeToOptions = new ObservableCollection<string>(validOptions);
        }
        
        private List<ColorOption> GetColorOptionList(ParsingProfileModel parsingProfile, MarkupTypeModel? selfMarkupType = null)
        {
            Type type = typeof(Colors);
            List<ColorOption> colorOptions = new List<ColorOption>();
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType == typeof(Color))
                {
                    var color = (Color)property.GetValue(null)!;
                    if (parsingProfile.MarkupTypes.Any(markupType => markupType.Color.Equals(color) || (selfMarkupType.HasValue && markupType != selfMarkupType)))
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