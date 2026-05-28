using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public class ColorSimilarityTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public override Type ViewModelType => typeof(ColorSimiliarityTextComparisonConfigurationView);

        public override Type ServiceType => typeof(ColorSimilarityTextComparisonService);
        
        public override string DisplayName => "Color Similarity";
        
        
        public override void ApplyChanges()
        {
            throw new NotImplementedException();
        }

        public override void ResetChanges()
        {
            throw new NotImplementedException();
        }
    }
}