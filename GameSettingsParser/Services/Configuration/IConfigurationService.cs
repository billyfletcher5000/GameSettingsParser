using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.Services.Configuration
{
    public interface IConfigurationService
    {
        public T? GetConfiguration<T>() where T : class, IConfigurationModel;
        public IConfigurationModel[] GetAllConfigurations();
        public void SaveConfiguration(IConfigurationModel configuration, ConfigurationScope scope);
        
        public void RegisterConfigurationSource(IConfigurationSource source, ConfigurationScope scope);
        public void UnregisterConfigurationSource(IConfigurationSource source);
    }
}