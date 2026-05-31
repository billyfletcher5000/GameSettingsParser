using GameSettingsParser.Constants;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public abstract class BasicTextComparisonConfigurationModel : ITextComparisonConfigurationModel
    {
        public float MinimumConfidence { get; set; } = 0.0f;

        public abstract Type ViewModelType { get; }
        
        public string? Section => $"{ConfigurationSectionConstants.Project}/{ConfigurationSectionConstants.TextComparison}";

        public abstract Type ServiceType { get; }
        
        public abstract string DisplayName { get; }
    }
}