using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Views.Configuration.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public abstract class BasicTextComparisonConfigurationViewModel : BindableBase, IConfigurationViewModel
    {
        public virtual IConfigurationModel? Configuration { get; set; }
        
        public virtual string DisplayName => Configuration?.DisplayName ?? "Text Comparison";
        
        public BasicTextComparisonConfigurationModel? ThisConfiguration => Configuration as BasicTextComparisonConfigurationModel;

        private float _minimumConfidence = 0.0f;
        public float MinimumConfidence
        {
            get => _minimumConfidence;
            set => SetProperty(ref _minimumConfidence, value);
        }

        public virtual Type ViewType => typeof(BasicTextComparisonConfigurationView);
        
        public virtual void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.MinimumConfidence = MinimumConfidence;
        }

        public virtual void Initialise()
        {
            MinimumConfidence = ThisConfiguration?.MinimumConfidence ?? 0.0f;
        }

        public virtual bool CheckForChanges()
        {
            if (ThisConfiguration == null)
                return false;
            
            return Math.Abs(MinimumConfidence - ThisConfiguration.MinimumConfidence) > float.Epsilon;
        }
    }
}