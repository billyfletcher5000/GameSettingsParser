using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public interface ITextComparisonConfigurationModel : IConfigurationModel
    {
        public Type ServiceType { get; }
    }
}