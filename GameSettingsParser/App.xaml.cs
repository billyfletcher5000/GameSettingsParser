using System.Windows;
using GameSettingsParser.ServiceProviders.AnalysisExport;
using GameSettingsParser.ServiceProviders.TextComparison;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.Authentication;
using GameSettingsParser.Services.Configuration;
using GameSettingsParser.Services.Confluence;
using GameSettingsParser.Services.ImageAnalysis;
using GameSettingsParser.Services.KeyVault;
using GameSettingsParser.Services.Logging;
using GameSettingsParser.Services.Progress;
using GameSettingsParser.Services.SessionStore;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Services.UserState;
using GameSettingsParser.Services.Validation;
using GameSettingsParser.Services.Windows;
using GameSettingsParser.Settings;
using GameSettingsParser.Utility;
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
        containerRegistry.RegisterSingleton<ILogService, ConsoleLogService>();
        containerRegistry.RegisterSingleton<IConfigurationService, ConfigurationService>();
        containerRegistry.RegisterSingleton<IImageAnalysisService, TesseractImageAnalysisService>();
        containerRegistry.RegisterSingleton<ITextComparisonService, CombinedTextComparisonService>();
        containerRegistry.RegisterSingleton<IProfileValidationService, BasicProfileValidationService>();
        containerRegistry.RegisterSingleton<IAuthenticationService, OAuth2AuthenticationService>();
        containerRegistry.RegisterSingleton<IUserStateService, BasicUserStateService>();
        containerRegistry.RegisterSingleton<IKeyVaultService, EnvironmentVariableVaultService>();
        containerRegistry.RegisterSingleton<ISessionStoreService, AppSettingsSessionStoreService>();
        containerRegistry.RegisterSingleton<IWindowService, BasicWindowService>();
        containerRegistry.RegisterSingleton<IProgressDialogService, ProgressDialogService>();
        containerRegistry.RegisterSingleton<ConfluenceApiService>();
        
        containerRegistry.RegisterSingleton<IAnalysisExportServiceProvider, AnalysisExportServiceProvider>();
        RegisterSwitchableServices<IAnalysisExportService>(containerRegistry);
        
        containerRegistry.RegisterSingleton<ITextComparisonServiceProvider, TextComparisonServiceProvider>();
        RegisterSwitchableServices<ITextComparisonService>(containerRegistry);
    }

    private static void RegisterSwitchableServices<T>(IContainerRegistry containerRegistry) where T : class
    {
        var analysisExportServiceImplementations = SwitchableServiceHelper.GetSwitchableServiceImplementations<T>();
        foreach (var implementation in analysisExportServiceImplementations)
        {
            var serviceId = SwitchableServiceHelper.GetSwitchableServiceId(implementation);
            if (serviceId != null)
                containerRegistry.Register(typeof(T), implementation, serviceId);
        }
    }

    protected override Window CreateShell()
    {
        var window = Container.Resolve<MainWindow>();
        return window;
    }

    protected override void Initialize()
    {
        base.Initialize();

        var configurationService = Container.Resolve<IConfigurationService>();
        RegisterConfigurations(configurationService);
        
        
    }

    protected override void OnExit(ExitEventArgs e)
    {
        UserSettings.Save(SettingsPathHelper.GetSettingsFilePath());
        base.OnExit(e);
    }

    protected void RegisterConfigurations(IConfigurationService configurationService)
    {
        configurationService.RegisterConfigurationSource(UserSettings.Instance, ConfigurationScope.UserSettings);
    }
}