using System.Collections.ObjectModel;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.Model.TextComparisonConfiguration
{
    public class CombinedTextComparisonConfigurationModel : BasicTextComparisonConfigurationModel
    {
        public struct Configuration
        {
            public ITextComparisonConfigurationModel ConfigurationModel { get; init; }
            public double Weight { get; set; } = 1.0f;

            public Configuration(ITextComparisonConfigurationModel configurationModel, double weight)
            {
                ConfigurationModel = configurationModel;
                Weight = weight;
            }
        }
        
        public ObservableCollection<Configuration>? Configurations { get; set; } = [];
        
        public override Type ServiceType => typeof(CombinedTextComparisonService);
        
        public override string DisplayName => "Weighted Combination";

        public CombinedTextComparisonConfigurationModel()
        {
            Configurations.CollectionChanged += (sender, args) => HasChanges = true;
        }
    }
}