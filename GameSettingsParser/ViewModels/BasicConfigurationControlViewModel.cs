using GameSettingsParser.Model.TextComparisonConfiguration;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ViewModels
{
    public class BasicConfigurationControlViewModel : BindableBase, INavigationAware
    {
        private BasicTextComparisonConfigurationModel? _basicConfigModel;
        public BasicTextComparisonConfigurationModel? BasicConfigModel
        {
            get => _basicConfigModel;
            set
            {
                if (SetProperty(ref _basicConfigModel, value))
                {
                    RaisePropertyChanged(nameof(MinimumConfidence));
                }
            }
        }

        public float MinimumConfidence
        {
            get => BasicConfigModel?.MinimumConfidence ?? 0.0f;
            set
            {
                if (BasicConfigModel != null) 
                    BasicConfigModel.MinimumConfidence = value;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.ContainsKey(TextComparisonServiceConstants.NavigationParameterKey))
                return;
            
            var model = navigationContext.Parameters[TextComparisonServiceConstants.NavigationParameterKey] as BasicTextComparisonConfigurationModel;

            BasicConfigModel = model ?? throw new System.ArgumentException("The navigation parameter must be a valid CombinedTextComparisonConfigurationModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.ContainsKey(TextComparisonServiceConstants.NavigationParameterKey))
                return false;
            
            var basicConfigModel = navigationContext.Parameters[TextComparisonServiceConstants.NavigationParameterKey] as BasicTextComparisonConfigurationModel;
            return basicConfigModel != null;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}