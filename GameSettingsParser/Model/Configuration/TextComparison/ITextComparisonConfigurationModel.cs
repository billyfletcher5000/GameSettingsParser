using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public interface ITextComparisonConfigurationModel : IConfigurationModel
    {
        public bool HasChanges { get; set; }
        
        public Type ServiceType { get; }
        
        public string DisplayName { get; }
    }
}