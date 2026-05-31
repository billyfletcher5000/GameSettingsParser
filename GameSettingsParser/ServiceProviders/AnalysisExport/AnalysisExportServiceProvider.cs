using GameSettingsParser.Model.Configuration.General;
using GameSettingsParser.Model.Configuration.Project;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.Configuration;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ServiceProviders.AnalysisExport
{
    public class AnalysisExportServiceProvider : IAnalysisExportServiceProvider
    {
        private readonly IContainerProvider _containerProvider;
        private readonly IConfigurationService _configurationService;

        private IAnalysisExportService? _current;

        public event Action<IAnalysisExportService>? CurrentChanged;

        public AnalysisExportServiceProvider(
            IContainerProvider containerProvider,
            IConfigurationService configurationService)
        {
            _containerProvider = containerProvider;
            _configurationService = configurationService;
            
            var projectConfig = _configurationService.GetConfiguration<ProjectConfigurationModel>();
                
            if(projectConfig == null)
                throw new InvalidOperationException("Text comparison service is not configured");

            projectConfig.OnAnalysisExportServiceIdChanged += OnConfigurationChanged;
        }

        public IAnalysisExportService Current
        {
            get
            {
                if (_current is not null)
                    return _current;

                var projectConfig = _configurationService.GetConfiguration<ProjectConfigurationModel>();
                
                if(projectConfig?.AnalysisExportServiceId == null)
                    throw new InvalidOperationException("Text comparison service is not configured");

                _current = _containerProvider.Resolve<IAnalysisExportService>(projectConfig.AnalysisExportServiceId);

                return _current;
            }
        }

        private void OnConfigurationChanged(string? sender)
        {
            _current = null;
            CurrentChanged?.Invoke(Current);
        }
    }
}