using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GameSettingsParser.Services.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public class ConfigurationDialogViewModel : BindableBase
    {
        public ObservableCollection<IConfigurationTreeViewItem> TreeViewItems { get; } = [];
        
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
        
        public ConfigurationDialogViewModel(IConfigurationService configurationService)
        {
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
                            parentSection = sectionViewModel;
                        }
                        
                        section = sectionViewModel;
                    }
                }
                
                if (Activator.CreateInstance(configurationModel.ViewModelType) is IConfigurationViewModel
                    configurationViewModel && section != null)
                {
                    configurationViewModel.Configuration = configurationModel;
                    configurationViewModel.ResetChanges();
                    section.TreeViewItems.Add(configurationViewModel);

                    if(configurationViewModel is BindableBase bindableBase)
                        bindableBase.PropertyChanged += (_, _) => CalculateHasChanges();
                }
            }
        }

        private void OnTreeViewSelectionChanged(object selection)
        {
            SelectedConfiguration = selection as IConfigurationViewModel;
        }

        private void OnOK(Window window)
        {
            OnApply();
            window.DialogResult = true;
        }

        private void OnCancel(Window window)
        {
            SelectedConfiguration?.ResetChanges();
            window.DialogResult = false;
        }

        private void OnApply()
        {
            SelectedConfiguration?.ApplyChanges();
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