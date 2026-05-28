using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public class ColorSimilarityConfigurationViewModel : BasicTextComparisonConfigurationViewModel
    {
        public override Type ViewType => typeof(ColorSimiliarityTextComparisonConfigurationView);
    }
}