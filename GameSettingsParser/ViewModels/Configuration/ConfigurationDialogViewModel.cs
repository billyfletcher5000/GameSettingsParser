using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Services.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public class ConfigurationDialogViewModel : BindableBase
    {
        public ObservableCollection<IConfigurationTreeViewItem> TreeViewItems { get; } = [];
        
        private ConfigurationViewModelBase? _selectedSection;
        public ConfigurationViewModelBase? SelectedConfiguration
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                RaisePropertyChanged();
            } 
        }

        private bool _hasChanges = false;
        public bool HasChanges
        {
            get => _hasChanges;
            private set => SetProperty(ref _hasChanges, value);
        }

        public ICommand OnOkCommand { get; }
        public ICommand OnCancelCommand { get; }
        public ICommand OnApplyCommand { get; }
        public ICommand OnTreeViewSelectionChangedCommand { get; }
        
        private IConfigurationService _configurationService;
        private List<IConfigurationModel> _changedConfigurations = [];
        
        public ConfigurationDialogViewModel(IConfigurationService configurationService, IContainerProvider containerProvider)
        {
            _configurationService = configurationService;
            
            OnOkCommand = new DelegateCommand<Window>(OnOK);
            OnCancelCommand = new DelegateCommand<Window>(OnCancel);
            OnApplyCommand = new DelegateCommand(OnApply);
            
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
                        TreeViewItems.Insert(0, generalSection);
                    }
                    
                    section = generalSection;
                }
                else
                {
                    var separatedSections = configurationModel.Section.Split('/');
                    ConfigurationSectionViewModel? parentSection = null;

                    foreach (var sectionName in separatedSections)
                    {
                        var sectionsList = parentSection?.TreeViewItems ?? TreeViewItems;
                        var sectionViewModel = sectionsList.Select(s => s as ConfigurationSectionViewModel).FirstOrDefault(s => s?.DisplayName == sectionName);
                        
                        if (sectionViewModel == null)
                        {
                            sectionViewModel = new ConfigurationSectionViewModel() { DisplayName = sectionName };
                            sectionsList.Add(sectionViewModel);
                        }
                        
                        parentSection = sectionViewModel;
                        section = sectionViewModel;
                    }
                }
                
                if (containerProvider.Resolve(configurationModel.ViewModelType) is ConfigurationViewModelBase
                    configurationViewModel && section != null)
                {
                    configurationViewModel.Configuration = configurationModel;
                    section.TreeViewItems.Add(configurationViewModel);
                    configurationViewModel.OnConfigurationChanged += OnConfigurationChanged;
                }
            }
        }

        private void OnConfigurationChanged(IConfigurationModel? configuration)
        {
            if (configuration == null)
                return;
            
            _changedConfigurations.Add(configuration);
            CalculateHasChanges();
        }

        private void OnTreeViewSelectionChanged(object selection)
        {
            SelectedConfiguration = selection as ConfigurationViewModelBase;
        }

        private void OnOK(Window window)
        {
            OnApply();
            window.DialogResult = true;
        }

        private void OnCancel(Window window)
        {
            window.DialogResult = false;
        }

        private void OnApply()
        {
            foreach (var configurationTreeViewItem in TreeViewItems)
            {
                configurationTreeViewItem.ApplyChanges();
            }
            
            foreach (var configuration in _changedConfigurations)
                _configurationService.NotifyConfigurationChangesApplied(configuration);
            
            CalculateHasChanges();
        }

        private void CalculateHasChanges()
        {
            foreach (var treeViewItem in TreeViewItems)
            {
                if (treeViewItem.CheckForChanges())
                {
                    HasChanges = true;
                    return;
                }
            }
            
            HasChanges = false;
        }
    }
}