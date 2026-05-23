using System.Windows;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.Authentication;
using GameSettingsParser.Services.Confluence;
using GameSettingsParser.Services.ImageAnalysis;
using GameSettingsParser.Services.KeyVault;
using GameSettingsParser.Services.Logging;
using GameSettingsParser.Services.SessionStore;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Services.UserState;
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
        containerRegistry.Register<ILogService, ConsoleLogService>();
        containerRegistry.Register<IAnalysisExportService, ConfluenceAnalysisExportService>();
        containerRegistry.Register<IImageAnalysisService, TesseractImageAnalysisService>();
        containerRegistry.Register<ITextComparisonService, CombinedTextComparisonService>();
        containerRegistry.Register<IProfileValidationService, BasicProfileValidationService>();
        containerRegistry.Register<IAuthenticationService, OAuth2AuthenticationService>();
        containerRegistry.Register<IUserStateService, BasicUserStateService>();
        containerRegistry.Register<IKeyVaultService, EnvironmentVariableVaultService>();
        containerRegistry.Register<ISessionStoreService, AppSettingsSessionStoreService>();
        containerRegistry.Register<ConfluenceApiService>();
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