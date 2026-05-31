using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.ViewModels.Configuration.TextComparison;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public class GoogleViTTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public override Type ViewModelType => typeof(BasicTextComparisonConfigurationViewModel);

        public override Type ServiceType => typeof(GoogleViTTextComparisonService);

        public override string DisplayName => "Google ViT Font Feature Detection";
    }
}