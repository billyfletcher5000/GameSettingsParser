using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GameSettingsParser.Controls.TextComparison;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Utility;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public class CombinedTextComparisonWeightedConfigurationViewModel : BindableBase
    {
        public CombinedTextComparisonConfigurationModel.WeightedConfiguration Model { get; }
        
        private ConfigurationViewModelBase? _viewModel;
        public ConfigurationViewModelBase? ViewModel
        {
            get => _viewModel;
            set => SetProperty(ref _viewModel, value);
        }

        private string? _configurationModelId;
        public string? ConfigurationModelId
        {
            get => _configurationModelId;
            set
            {
                if (SetProperty(ref _configurationModelId, value))
                {
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }
        
        private string? _displayName;
        public string? DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }
        
        private float _weight = 1.0f;
        public float Weight { get => _weight; set => SetProperty(ref _weight, value); }

        public CombinedTextComparisonWeightedConfigurationViewModel(CombinedTextComparisonConfigurationModel.WeightedConfiguration model, ConfigurationViewModelBase viewModel)
        {
            Model = model;
            ConfigurationModelId = SwitchableServiceHelper.GetSwitchableServiceId(model.ConfigurationModel.ServiceType);
            DisplayName = model.DisplayName;
            Weight = model.Weight;
            ViewModel = viewModel;
        }

        public bool CheckForChanges()
        {
            return Math.Abs(Model.Weight - Weight) > float.Epsilon;
        }
    }
    
    public class CombinedTextComparisonConfigurationViewModel : BasicTextComparisonConfigurationViewModel
    {
        public override Type ViewType => typeof(CombinedTextComparisonConfigurationView);

        public override string DisplayName => ThisConfiguration?.DisplayName ?? "Weighted Combination";

        public new CombinedTextComparisonConfigurationModel? ThisConfiguration => Configuration as CombinedTextComparisonConfigurationModel;
        
        public ObservableCollection<CombinedTextComparisonWeightedConfigurationViewModel> ChildConfigurations { get; } = [];
        
        private CombinedTextComparisonWeightedConfigurationViewModel? _selectedChildConfiguration;

        public CombinedTextComparisonWeightedConfigurationViewModel? SelectedChildConfiguration
        {
            get => _selectedChildConfiguration;
            set
            {
                if (SetProperty(ref _selectedChildConfiguration, value))
                {
                    RaisePropertyChanged(nameof(SelectedChildConfigurationViewModel));
                    ((DelegateCommand)RemoveChildConfigurationCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        public ConfigurationViewModelBase? SelectedChildConfigurationViewModel => SelectedChildConfiguration?.ViewModel;

        private readonly Dictionary<string, Type> _availableTextComparisonTypes = new();
        public IEnumerable<string> AvailableTextComparisonTypes => _availableTextComparisonTypes.Keys;
        
        private string? _selectedTextComparisonType;
        public string? SelectedTextComparisonType
        {
            get => _selectedTextComparisonType;
            set => SetProperty(ref _selectedTextComparisonType, value);
        }
        
        public ICommand AddNewChildConfigurationCommand { get; }
        public ICommand RemoveChildConfigurationCommand { get; }
        
        private readonly IContainerProvider _containerProvider;

        public CombinedTextComparisonConfigurationViewModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            
            AddNewChildConfigurationCommand = new DelegateCommand(AddNewChildConfiguration);
            RemoveChildConfigurationCommand = new DelegateCommand(RemoveChildConfiguration, () => SelectedChildConfiguration != null);

            var types = SwitchableServiceHelper.GetSwitchableServiceImplementations<ITextComparisonService>();
            foreach (var type in types)
            {
                var displayName = SwitchableServiceHelper.GetSwitchableServiceDisplayName(type);
                if (displayName != null)
                    _availableTextComparisonTypes.Add(displayName, type);
            }
            
            SelectedTextComparisonType = _availableTextComparisonTypes.First().Key;
        }

        private void AddNewChildConfiguration()
        {
            if (SelectedTextComparisonType == null)
                return;

            var type = _availableTextComparisonTypes[SelectedTextComparisonType];
            var serviceId = SwitchableServiceHelper.GetSwitchableServiceId(type);
            if (serviceId == null)
                return;

            var service = _containerProvider.Resolve<ITextComparisonService>(serviceId);
            if (service == null)
                return;

            if (_containerProvider.Resolve(service.ConfigurationType) is not ITextComparisonConfigurationModel config)
                return;
            
            var displayName = GetUniqueDisplayName(config.DisplayName);
            var weightedEntry = new CombinedTextComparisonConfigurationModel.WeightedConfiguration(config, displayName, 1.0f);
            var viewModel = _containerProvider.Resolve(config.ViewModelType) as ConfigurationViewModelBase;

            if (viewModel == null)
                return;

            viewModel.Configuration = config;
            var weightedConfigViewModel = new CombinedTextComparisonWeightedConfigurationViewModel(weightedEntry, viewModel);
            weightedConfigViewModel.PropertyChanged += OnWeightedConfigPropertyChanged;
            ChildConfigurations.Add(weightedConfigViewModel);
            RaiseConfigurationChanged();
        }

        private void RemoveChildConfiguration()
        {
            if (SelectedChildConfiguration == null)
                return;
            
            SelectedChildConfiguration.PropertyChanged -= OnWeightedConfigPropertyChanged;
            ChildConfigurations.Remove(SelectedChildConfiguration);
            RaiseConfigurationChanged();
        }
        
        public override void ApplyChanges()
        {
            base.ApplyChanges();

            foreach (var childConfiguration in ChildConfigurations)
            {
                childConfiguration.Model.DisplayName = childConfiguration.DisplayName!;
                childConfiguration.Model.Weight = childConfiguration.Weight;
            }
            
            ThisConfiguration?.ChildConfigurations.Clear();
            ThisConfiguration?.ChildConfigurations.AddRange(ChildConfigurations.Select(vm => vm.Model));
        }

        protected override void OnConfigurationUpdated()
        {
            UpdateChildConfigurations();
            RaisePropertyChanged(nameof(ChildConfigurations));
        }
        
        private void UpdateChildConfigurations()
        {
            if (ThisConfiguration == null)
            {
                ChildConfigurations.Clear();
                return;
            }

            ChildConfigurations.RemoveAll(vm => ThisConfiguration.ChildConfigurations.All(c => c != vm.Model));
            
            foreach (var childConfiguration in ThisConfiguration.ChildConfigurations)
            {
                if(ChildConfigurations!.Any(vm => vm.Model == childConfiguration))
                    continue;
                
                var viewModel = _containerProvider.Resolve(childConfiguration.ConfigurationModel.ViewModelType) as ConfigurationViewModelBase;

                if (viewModel == null)
                    return;

                viewModel.Configuration = childConfiguration.ConfigurationModel;
                var weightedConfigViewModel = new CombinedTextComparisonWeightedConfigurationViewModel(childConfiguration, viewModel);
                weightedConfigViewModel.PropertyChanged += OnWeightedConfigPropertyChanged;
                ChildConfigurations?.Add(weightedConfigViewModel);
            }
        }

        public override bool CheckForChanges()
        {
            if (ThisConfiguration == null)
                return false;

            foreach (var childViewModel in ChildConfigurations)
            {
                if (childViewModel.CheckForChanges())
                    return true;
                
                if (ThisConfiguration?.ChildConfigurations.Any(c => c == childViewModel.Model) == false)
                    return true;
            }
            
            return base.CheckForChanges();
        }

        private string GetUniqueDisplayName(string displayName)
        {
            if(ChildConfigurations.All(vm => vm.DisplayName != displayName))
                return displayName;
            
            var index = 2;
            while(ChildConfigurations.Any(vm => vm.DisplayName == $"{displayName} ({index})"))
                index++;
            
            return $"{displayName} ({index})";
        }
        
        private void OnWeightedConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseConfigurationChanged();
        }
    }
}