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
        private ProjectConfigurationModel? _projectConfig;

        public event Action<ITextComparisonService?>? CurrentChanged;

        public TextComparisonServiceProvider(
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
                _projectConfig.OnTextComparisonServiceIdChanged -= OnConfigurationChanged;
            
            _projectConfig = _configurationService.GetConfiguration<ProjectConfigurationModel>();
            
            if (_projectConfig != null)
                _projectConfig.OnTextComparisonServiceIdChanged += OnConfigurationChanged;

            OnConfigurationChanged();
        }

        public ITextComparisonService? Current
        {
            get
            {
                if (_current is not null)
                    return _current;

                if (_projectConfig?.TextComparisonServiceId == null)
                    return null;

                _current = _containerProvider.Resolve<ITextComparisonService>(_projectConfig.TextComparisonServiceId);

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