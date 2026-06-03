using Newtonsoft.Json;

namespace GameSettingsParser.Model.Configuration
{
    public interface IConfigurationModel
    { 
        [JsonIgnore]
        public string DisplayName { get; }
        [JsonIgnore]
        public Type ViewModelType { get; }
        [JsonIgnore]
        public string? Section { get; }
    }
}