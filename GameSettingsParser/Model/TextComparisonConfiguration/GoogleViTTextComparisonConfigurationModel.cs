using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.Model.TextComparisonConfiguration
{
    public class GoogleViTTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public override Type ServiceType => typeof(GoogleViTTextComparisonService);

        public override string DisplayName => "Google ViT Font Feature Detection";
    }
}