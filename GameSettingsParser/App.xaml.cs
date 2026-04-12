using System.Configuration;
using System.Data;
using System.Windows;
using GameSettingsParser.Settings;
using GameSettingsParser.ViewModels;

namespace GameSettingsParser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override void OnStartup(StartupEventArgs e)
    {
        UserSettings.Load(SettingsPathHelper.GetSettingsFilePath());
        base.OnStartup(e);
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
    }

    protected override Window CreateShell()
    {
        var window = Container.Resolve<MainWindow>();
        return window;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        UserSettings.Save(SettingsPathHelper.GetSettingsFilePath());
        base.OnExit(e);
    }
}