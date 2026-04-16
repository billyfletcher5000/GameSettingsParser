using System.Windows;
using GameSettingsParser.Services.DataExport;
using GameSettingsParser.Services.ImageAnalysis;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Services.Validation;
using GameSettingsParser.Settings;
using GameSettingsParser.ViewModels;
using Prism.Unity;

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
        containerRegistry.Register<IDataExportService, DebugDataExportService>();
        containerRegistry.Register<IImageAnalysisService, TesseractImageAnalysisService>();
        containerRegistry.Register<ITextComparisonService, CombinedTextComparisonService>();
        containerRegistry.Register<IProfileValidationService, BasicProfileValidationService>();
    }

    protected override Window CreateShell()
    {
        var window = Container.Resolve<MainWindow>();
        return window;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Container.Resolve<MainWindowViewModel>().Save();
        UserSettings.Save(SettingsPathHelper.GetSettingsFilePath());
        base.OnExit(e);
    }
}