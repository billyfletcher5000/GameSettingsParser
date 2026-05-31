using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public class ColorSimilarityConfigurationViewModel : BasicTextComparisonConfigurationViewModel
    {
        public override string DisplayName => ThisConfiguration?.DisplayName ?? "Color Similarity";
    }
}