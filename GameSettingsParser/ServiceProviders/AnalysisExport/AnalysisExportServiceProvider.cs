using GameSettingsParser.Model.Configuration.Project;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.Configuration;

namespace GameSettingsParser.ServiceProviders.AnalysisExport
{
    public class AnalysisExportServiceProvider : IAnalysisExportServiceProvider
    {
        private readonly IContainerProvider _containerProvider;
        private readonly IConfigurationService _configurationService;

        private IAnalysisExportService? _current;
        private ProjectConfigurationModel? _projectConfig;

        public event Action<IAnalysisExportService?>? CurrentChanged;

        public AnalysisExportServiceProvider(
            IContainerProvider containerProvider,
            IConfigurationService configurationService)
        {
            _containerProvider = containerProvider;
            _configurationService = configurationService;

            _configurationService.OnConfigurationSourcesChanged += UpdateProjectConfiguration;
            UpdateProjectConfiguration();
        }

        private void UpdateProjectConfiguration()
        {
            if (_projectConfig != null)
                _projectConfig.OnAnalysisExportServiceIdChanged -= OnConfigurationChanged;
            
            _projectConfig = _configurationService.GetConfiguration<ProjectConfigurationModel>();
            
            if (_projectConfig != null)
                _projectConfig.OnAnalysisExportServiceIdChanged += OnConfigurationChanged;

            OnConfigurationChanged();
        }

        public IAnalysisExportService? Current
        {
            get
            {
                if (_current is not null)
                    return _current;

                if (_projectConfig?.AnalysisExportServiceId == null)
                    return null;

                _current = _containerProvider.Resolve<IAnalysisExportService>(_projectConfig.AnalysisExportServiceId);

                return _current;
            }
        }

        private void OnConfigurationChanged(string? sender = null)
        {
            _current = null;
            CurrentChanged?.Invoke(Current);
        }
    }
}