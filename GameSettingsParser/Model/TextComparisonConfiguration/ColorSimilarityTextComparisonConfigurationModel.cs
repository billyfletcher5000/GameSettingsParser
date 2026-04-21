using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.Model.TextComparisonConfiguration
{
    public class ColorSimilarityTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public override Type ServiceType => typeof(ColorSimilarityTextComparisonService);
        
        public override string DisplayName => "Color Similarity";
    }
}