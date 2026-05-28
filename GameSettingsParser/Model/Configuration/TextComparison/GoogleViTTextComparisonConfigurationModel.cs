using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public class GoogleViTTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public override Type ViewModelType => typeof(GoogleViTTextComparisonConfigurationView);

        public override Type ServiceType => typeof(GoogleViTTextComparisonService);

        public override string DisplayName => "Google ViT Font Feature Detection";
        
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