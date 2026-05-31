using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.ViewModels.Configuration.TextComparison;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public class ColorSimilarityTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public override Type ViewModelType => typeof(BasicTextComparisonConfigurationViewModel);

        public override Type ServiceType => typeof(ColorSimilarityTextComparisonService);
        
        public override string DisplayName => "Color Similarity";
    }
}