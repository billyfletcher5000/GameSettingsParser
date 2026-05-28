using GameSettingsParser.Model.Configuration.General;
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
            
            var generalConfigModel = _configurationService.GetConfiguration<GeneralConfigurationModel>();
                
            if(generalConfigModel == null)
                throw new InvalidOperationException("Text comparison service is not configured");

            generalConfigModel.AnalysisExportServiceIdChanged += OnConfigurationChanged;
        }

        public IAnalysisExportService Current
        {
            get
            {
                if (_current is not null)
                    return _current;

                var generalConfigModel = _configurationService.GetConfiguration<GeneralConfigurationModel>();
                
                if(generalConfigModel == null || generalConfigModel.AnalysisExportServiceId == null)
                    throw new InvalidOperationException("Text comparison service is not configured");

                _current = _containerProvider.Resolve<IAnalysisExportService>(generalConfigModel.AnalysisExportServiceId);

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