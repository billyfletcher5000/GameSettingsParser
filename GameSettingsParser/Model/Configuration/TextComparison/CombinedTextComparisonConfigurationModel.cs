using System.Collections.ObjectModel;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.ViewModels.Configuration.TextComparison;

namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public class CombinedTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public class WeightedConfiguration
        {
            public ITextComparisonConfigurationModel ConfigurationModel { get; init; }
            public string DisplayName { get; set; }
            public float Weight { get; set; }

            public WeightedConfiguration(ITextComparisonConfigurationModel configurationModel, string displayName, float weight)
            {
                ConfigurationModel = configurationModel;
                DisplayName = displayName;
                Weight = weight;
            }
        }
        
        public ObservableCollection<WeightedConfiguration> ChildConfigurations { get; set; } = [];

        public override Type ViewModelType => typeof(CombinedTextComparisonConfigurationViewModel);
        public override Type ServiceType => typeof(CombinedTextComparisonService);
        public override string DisplayName => "Weighted Combination";
    }
}