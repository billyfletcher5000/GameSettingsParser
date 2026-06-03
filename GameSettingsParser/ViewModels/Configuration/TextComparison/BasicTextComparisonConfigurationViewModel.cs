using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public abstract class BasicTextComparisonConfigurationViewModel : ConfigurationViewModelBase
    {
        public override string DisplayName => Configuration?.DisplayName ?? "Text Comparison";
        
        public BasicTextComparisonConfigurationModel? ThisConfiguration => Configuration as BasicTextComparisonConfigurationModel;

        private float _minimumConfidence = 0.0f;
        public float MinimumConfidence
        {
            get => _minimumConfidence;
            set => SetProperty(ref _minimumConfidence, value);
        }

        public override Type ViewType => typeof(BasicTextComparisonConfigurationView);
        
        public override void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.MinimumConfidence = MinimumConfidence;
        }

        protected override void OnConfigurationUpdated()
        {
            MinimumConfidence = ThisConfiguration?.MinimumConfidence ?? 0.0f;
        }

        public override bool CheckForChanges()
        {
            if (ThisConfiguration == null)
                return false;
            
            return Math.Abs(MinimumConfidence - ThisConfiguration.MinimumConfidence) > float.Epsilon;
        }
    }
}