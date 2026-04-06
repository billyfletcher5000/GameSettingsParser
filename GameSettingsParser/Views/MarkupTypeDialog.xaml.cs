using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using GameSettingsParser.Model;

namespace GameSettingsParser.Views;

public partial class MarkupTypeDialog : Window
{
    public sealed class ColorOption
    {
        public string Name { get; init; } = "";
        public Color Color { get; init; }
    }
    
    public static readonly DependencyProperty TypeNameProperty =
        DependencyProperty.Register(nameof(TypeName),
            typeof(string), typeof(MarkupTypeDialog), new PropertyMetadata(OnTypeNameChanged));

    public string TypeName
    {
        get => (string)GetValue(TypeNameProperty);
        set => SetValue(TypeNameProperty, value);
    }
    
    public static readonly DependencyProperty TypeColorProperty =
        DependencyProperty.Register(nameof(TypeColor),
            typeof(ColorOption), typeof(MarkupTypeDialog), new PropertyMetadata(OnTypeBrushChanged));
    
    public ColorOption TypeColor
    {
        get => (ColorOption)GetValue(TypeColorProperty);
        set => SetValue(TypeColorProperty, value);
    }
    
    public static readonly DependencyProperty IsDynamicProperty =
        DependencyProperty.Register(nameof(IsDynamic),
            typeof(bool), typeof(MarkupTypeDialog), new PropertyMetadata(false, OnIsDynamicChanged));
    
    public bool IsDynamic
    {
        get => (bool)GetValue(IsDynamicProperty);
        set => SetValue(IsDynamicProperty, value);
    }

    public MarkupTypeModel MarkupTypeModel => _markupType;
    private MarkupTypeModel _markupType;
    
    public ObservableCollection<ColorOption> ColorOptions { get; init; }
    
    public MarkupTypeDialog()
    {
        Title = "New Markup Type";
        
        ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList());
        _markupType.Color = Colors.Blue;
        TypeColor = ColorOptions.First(option => option.Color == _markupType.Color);
        InitializeComponent();
    }

    public MarkupTypeDialog(MarkupTypeModel markupTypeModel)
    {
        _markupType = markupTypeModel;
        Title = "Edit Markup Type";
        ColorOptions = new ObservableCollection<ColorOption>(GetColorOptionList());
        TypeColor = ColorOptions.First(option => option.Color == _markupType.Color);
        InitializeComponent();
    }
 
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
    
    // DependencyObject callbacks
    private static void OnTypeNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MarkupTypeDialog)d;
        control.HandleTypeNameChanged((string)e.NewValue);
    }

    private void HandleTypeNameChanged(string newValue)
    {
        _markupType.Name = newValue;
    }
    
    private static void OnTypeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MarkupTypeDialog)d;
        control.HandleTypeBrushChanged((ColorOption)e.NewValue);
    }

    private void HandleTypeBrushChanged(ColorOption newValue)
    {
        _markupType.Color = newValue.Color;
    }
    
    private static void OnIsDynamicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MarkupTypeDialog)d;
        control.HandleIsDynamicChanged((bool)e.NewValue);
    }

    private void HandleIsDynamicChanged(bool newValue)
    {
        _markupType.IsDynamic = newValue;
    }

    private static List<ColorOption> GetColorOptionList()
    {
        Type type = typeof(Colors);
        List<ColorOption> colorOptions = new List<ColorOption>();
        foreach (var property in type.GetProperties())
        {
            if (property.PropertyType == typeof(Color))
            {
                ColorOption colorOption = new ColorOption()
                {
                    Name = property.Name,
                    Color = (Color)property.GetValue(null)!
                };
                colorOptions.Add(colorOption);
            }
        }

        return colorOptions;
    }
}