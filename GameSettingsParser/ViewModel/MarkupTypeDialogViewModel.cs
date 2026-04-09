using System.Collections.ObjectModel;
using System.Windows.Media;
using GameSettingsParser.Model;

namespace GameSettingsParser.ViewModel
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
                _markupType.IsDynamic = value;
                RaisePropertyChanged();
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

        public ObservableCollection<ColorOption> ColorOptions { get; }
        public ObservableCollection<string> PositionedRelativeToOptions { get; init; }

        private MarkupTypeModel _markupType;
        public MarkupTypeModel MarkupTypeModel => _markupType;

        public MarkupTypeDialogViewModel(ParsingProfileModel parsingProfile)
        {
            _title = "New Markup Type";
            ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList(parsingProfile));
            _typeColor = ColorOptions.First();
            _markupType = new() { Color = _typeColor.Color };
            PositionedRelativeToOptions = new ObservableCollection<string>(parsingProfile.MarkupTypes.Select(type => type.Name));
        }

        public MarkupTypeDialogViewModel(ParsingProfileModel parsingProfile, MarkupTypeModel markupType)
        {
            _markupType = markupType;
            _title = "Edit Markup Type";
            ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList(parsingProfile));
            _typeColor = ColorOptions.First(option => option.Color == _markupType.Color);
            var validOptions = parsingProfile.MarkupTypes.Where(type => !type.Name.Equals(markupType.Name) && !type.PositionedRelativeTo.Equals(markupType.Name)).Select(type => type.Name).ToList();
            PositionedRelativeToOptions = new ObservableCollection<string>(validOptions);
        }
        
        private List<ColorOption> GetColorOptionList(ParsingProfileModel parsingProfile)
        {
            Type type = typeof(Colors);
            List<ColorOption> colorOptions = new List<ColorOption>();
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType == typeof(Color))
                {
                    var color = (Color)property.GetValue(null)!;
                    if (parsingProfile.MarkupTypes.Any(markupType => markupType.Color.Equals(color)))
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