using GameSettingsParser.Model.Configuration.General;
using GameSettingsParser.Model.Configuration.Project;
using GameSettingsParser.Services.Configuration;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ServiceProviders.TextComparison
{
    // TODO: Work out some way to code-gen this class or something along those lines,
    //       it needs to be compile time available but this is identical to AnalysisExportServiceProvider
    //       bar some minor changes for type and config variable name
    public class TextComparisonServiceProvider : ITextComparisonServiceProvider
    {
        private readonly IContainerProvider _containerProvider;
        private readonly IConfigurationService _configurationService;

        private ITextComparisonService? _current;

        public event Action<ITextComparisonService>? CurrentChanged;

        public TextComparisonServiceProvider(
            IContainerProvider containerProvider,
            IConfigurationService configurationService)
        {
            _containerProvider = containerProvider;
            _configurationService = configurationService;
            
            var projectConfig = _configurationService.GetConfiguration<ProjectConfigurationModel>();
                
            if(projectConfig == null)
                throw new InvalidOperationException("Text comparison service is not configured");

            projectConfig.OnTextComparisonServiceIdChanged += OnConfigurationChanged;
        }

        public ITextComparisonService Current
        {
            get
            {
                if (_current is not null)
                    return _current;

                var projectConfig = _configurationService.GetConfiguration<ProjectConfigurationModel>();
                
                if (projectConfig?.TextComparisonServiceId == null)
                    throw new InvalidOperationException("Text comparison service is not configured");

                _current = _containerProvider.Resolve<ITextComparisonService>(projectConfig.TextComparisonServiceId);

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