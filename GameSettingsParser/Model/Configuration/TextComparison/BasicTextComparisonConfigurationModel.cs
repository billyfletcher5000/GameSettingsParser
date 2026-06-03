using GameSettingsParser.Constants;
using Newtonsoft.Json;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public abstract class BasicTextComparisonConfigurationModel : ITextComparisonConfigurationModel
    {
        public float MinimumConfidence { get; set; } = 0.0f;

        [JsonIgnore]
        public abstract Type ViewModelType { get; }
        
        [JsonIgnore]
        public string? Section => $"{ConfigurationSectionConstants.Project}/{ConfigurationSectionConstants.TextComparison}";

        [JsonIgnore]
        public abstract Type ServiceType { get; }
        
        [JsonIgnore]
        public abstract string DisplayName { get; }
    }
}