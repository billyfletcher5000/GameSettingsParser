using System.Configuration;
using System.Data;
using System.Windows;
using GameSettingsParser.Settings;
using GameSettingsParser.ViewModel;

namespace GameSettingsParser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private MainWindowViewModel? _mainWindowViewModel;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow window = new MainWindow();
        UserSettings.Load(SettingsPathHelper.GetSettingsFilePath());
        _mainWindowViewModel = new MainWindowViewModel();
        window.DataContext = _mainWindowViewModel;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainWindowViewModel?.Save();
        UserSettings.Save(SettingsPathHelper.GetSettingsFilePath());
        base.OnExit(e);
    }
}