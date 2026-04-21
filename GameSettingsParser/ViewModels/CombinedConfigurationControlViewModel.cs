using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using GameSettingsParser.Attributes;
using GameSettingsParser.Model.TextComparisonConfiguration;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ViewModels
{
    public class CombinedConfigurationControlViewModel : BindableBase, INavigationAware
    {
        private CombinedTextComparisonConfigurationModel? _combinedConfigModel;
        public CombinedTextComparisonConfigurationModel? CombinedConfigModel
        {
            get => _combinedConfigModel;
            set
            {
                if (SetProperty(ref _combinedConfigModel, value))
                {
                    RaisePropertyChanged(nameof(Configurations));
                }
            }
        }
        
        public ObservableCollection<CombinedTextComparisonConfigurationModel.Configuration>? Configurations => CombinedConfigModel?.Configurations;
        
        private CombinedTextComparisonConfigurationModel.Configuration? _selectedConfiguration;

        public CombinedTextComparisonConfigurationModel.Configuration? SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (SetProperty(ref _selectedConfiguration, value))
                {
                    if (_selectedConfiguration != null)
                    {
                        var serviceType = _selectedConfiguration.Value.ConfigurationModel.ServiceType;
                        var regionKeyAttribute = serviceType.GetCustomAttribute(typeof(RegionNavigationKeyAttribute)) as RegionNavigationKeyAttribute;
                        if(regionKeyAttribute == null)
                            throw new ArgumentException($"ITextComparisonService type \"{serviceType.FullName}\" must have a RegionNavigationKeyAttribute to be configurable!");
                        _regionManager.RequestNavigate(RegionNames.CombinedTextConfigurationSelected, regionKeyAttribute.Key);
                    }
                }
            }
        }
        
        private IRegionManager _regionManager;

        public CombinedConfigurationControlViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.ContainsKey(TextComparisonServiceConstants.NavigationParameterKey))
                return;
            
            var model = navigationContext.Parameters[TextComparisonServiceConstants.NavigationParameterKey] as CombinedTextComparisonConfigurationModel;

            CombinedConfigModel = model ?? throw new System.ArgumentException("The navigation parameter must be a valid CombinedTextComparisonConfigurationModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.ContainsKey(TextComparisonServiceConstants.NavigationParameterKey))
                return false;
            
            var combinedConfigModel = navigationContext.Parameters[TextComparisonServiceConstants.NavigationParameterKey] as CombinedTextComparisonConfigurationModel;
            return combinedConfigModel != null;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}