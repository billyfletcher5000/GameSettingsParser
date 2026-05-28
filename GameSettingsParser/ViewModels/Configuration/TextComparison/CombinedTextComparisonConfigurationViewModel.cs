using System.Collections.ObjectModel;
using System.Reflection;
using GameSettingsParser.Attributes;
using GameSettingsParser.Controls.TextComparison;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public class CombinedTextComparisonConfigurationViewModel : BindableBase, IConfigurationViewModel
    {
        public Type ViewType => typeof(CombinedTextComparisonConfigurationView);
        
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
            set => SetProperty(ref _selectedConfiguration, value);
        }
        

        public CombinedTextComparisonConfigurationViewModel(IRegionManager regionManager)
        {
        }
    }
}