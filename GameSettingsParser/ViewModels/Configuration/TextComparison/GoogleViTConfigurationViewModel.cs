using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public class GoogleViTConfigurationViewModel : BasicTextComparisonConfigurationViewModel
    {
        public override string DisplayName => ThisConfiguration?.DisplayName ?? "Google ViT";
    }
}