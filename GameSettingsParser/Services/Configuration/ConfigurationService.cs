using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Services.Logging;

namespace GameSettingsParser.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly BidirectionalDictionary<ConfigurationScope, IConfigurationSource> _configurationSources = new();

        private readonly ILogService _log;
        
        public ConfigurationService(ILogService logService)
        {
            _log = logService;
        }
        
        public T? GetConfiguration<T>() where T : class, IConfigurationModel
        {
            // Ensure later versions override base configurations if they exist
            // note that this is based off the enum order being maintained as higher == more relevant
            var enumValues = Enum.GetValues<ConfigurationScope>();
            foreach (var scopeEnumValue in enumValues.Reverse())
            {
                if(!_configurationSources.TryGetValue(scopeEnumValue, out var configurationSource))
                    continue;

                return configurationSource.Configurations.OfType<T>().FirstOrDefault();
            }

            return null;
        }

        public IConfigurationModel[] GetAllConfigurations()
        {
            var configurations = new Dictionary<Type, IConfigurationModel>();
            
            foreach (var configurationSource in _configurationSources.Values)
            {
                foreach (var configuration in configurationSource.Configurations)
                    configurations[configuration.GetType()] = configuration;
            }
            
            return configurations.Values.ToArray();
        }

        public void SaveConfiguration(IConfigurationModel configuration, ConfigurationScope scope)
        {
            if (!_configurationSources.TryGetValue(scope, out var configurationSource))
            {
                _log.Warning($"Configuration source for scope {scope} is not registered, cannot add configuration: {configuration.GetType().Name}");
                return;
            }

            if (configurationSource.Configurations.Any(config => config.GetType() == configuration.GetType()))
                configurationSource.Configurations.Remove(configuration);
            
            configurationSource.Configurations.Add(configuration);
        }

        public void RegisterConfigurationSource(IConfigurationSource source, ConfigurationScope scope)
        {
            if (!_configurationSources.TryAdd(scope, source))
                throw new ArgumentException("Configuration source already registered for this scope");
            
            OnConfigurationSourcesChanged?.Invoke();
        }

        public void UnregisterConfigurationSource(IConfigurationSource source)
        {
            if (!_configurationSources.ContainsValue(source))
            {
                _log.Warning($"Configuration source {source} is not registered");
                return;
            }

            _configurationSources.Inverse.Remove(source);
            OnConfigurationSourcesChanged?.Invoke();
        }

        public event Action? OnConfigurationSourcesChanged;

        public void NotifyConfigurationChangesApplied(IConfigurationModel configuration)
        {
            var sourcesApplied = new HashSet<IConfigurationSource>();
            
            foreach (var configurationSource in _configurationSources.Values)
            {
                if (configurationSource.Configurations.Contains(configuration))
                    sourcesApplied.Add(configurationSource);
            }
            
            foreach(var source in sourcesApplied)
                source.OnConfigurationChangesApplied();
        }
    }
}