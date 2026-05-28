using System.Collections.ObjectModel;
using System.Windows.Input;
using GameSettingsParser.Services.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public class ConfigurationDialogViewModel : BindableBase
    {
        public ObservableCollection<ConfigurationSectionViewModel> Sections { get; } = [];
        
        private IConfigurationViewModel? _selectedSection;
        public IConfigurationViewModel? SelectedConfiguration
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                RaisePropertyChanged();
            } 
        }

        public ICommand OnTreeViewSelectionChangedCommand { get; }
        
        public ConfigurationDialogViewModel(IConfigurationService configurationService)
        {
            OnTreeViewSelectionChangedCommand = new DelegateCommand<object>(OnTreeViewSelectionChanged);
            
            var configurationModels = configurationService.GetAllConfigurations();

            ConfigurationSectionViewModel? generalSection = null;

            foreach (var configurationModel in configurationModels)
            {
                ConfigurationSectionViewModel? section = null;
                
                if (configurationModel.Section == null)
                {
                    if (generalSection == null)
                    {
                        generalSection = new ConfigurationSectionViewModel() { DisplayName = "General" };
                        Sections.Insert(0, generalSection);
                    }
                    
                    section = generalSection;
                }
                else
                {
                    section = Sections.FirstOrDefault(s => s.DisplayName == configurationModel.Section);
                    if (section == null)
                    {
                        section = new ConfigurationSectionViewModel() { DisplayName = configurationModel.Section };
                        Sections.Add(section);
                    }
                }

                if(Activator.CreateInstance(configurationModel.ViewModelType) is IConfigurationViewModel configurationViewModel)
                    section.Configurations.Add(configurationViewModel);
            }
        }

        private void OnTreeViewSelectionChanged(object selection)
        {
            SelectedConfiguration = selection as IConfigurationViewModel;
        }
    }
}