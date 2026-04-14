using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameSettingsParser.Settings;
using Microsoft.Win32;

namespace GameSettingsParser;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var windowSettings = UserSettings.Instance.MainWindowSettings;
        if (windowSettings != null)
        {
            this.WindowState = windowSettings.Value.WindowState;
        }
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var windowSettings = UserSettings.Instance.MainWindowSettings;
        if (windowSettings != null)
        {
            this.Left = windowSettings.Value.Left;
            this.Top = windowSettings.Value.Top;
            this.Width = windowSettings.Value.Width;
            this.Height = windowSettings.Value.Height;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        var windowSettings = UserSettings.Instance.MainWindowSettings ?? new UserSettings.WindowSettings();
        
        windowSettings.Left = this.Left;
        windowSettings.Top = this.Top;
        windowSettings.Width = this.Width;
        windowSettings.Height = this.Height;
        windowSettings.WindowState = this.WindowState;
        
        UserSettings.Instance.MainWindowSettings = windowSettings;
        
        base.OnClosed(e);
    }
}