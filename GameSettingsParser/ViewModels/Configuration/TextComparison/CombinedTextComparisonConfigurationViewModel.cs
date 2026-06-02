using System.Collections.ObjectModel;
using System.Windows.Input;
using GameSettingsParser.Controls.TextComparison;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Utility;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public class CombinedTextComparisonWeightedConfigurationViewModel : BindableBase
    {
        public CombinedTextComparisonConfigurationModel.WeightedConfiguration Model { get; }

        private string? _configurationModelId;
        public string? ConfigurationModelId
        {
            get => _configurationModelId;
            set => SetProperty(ref _configurationModelId, value);
        }
        
        private string? _configurationModelDisplayName;
        public string? ConfigurationModelDisplayName
        {
            get => _configurationModelDisplayName;
            set => SetProperty(ref _configurationModelDisplayName, value);
        }
        
        private float _weight = 1.0f;
        public float Weight { get => _weight; set => SetProperty(ref _weight, value); }

        public CombinedTextComparisonWeightedConfigurationViewModel(CombinedTextComparisonConfigurationModel.WeightedConfiguration model)
        {
            Model = model;
            ConfigurationModelId = SwitchableServiceHelper.GetSwitchableServiceId(model.ConfigurationModel.ServiceType);
            ConfigurationModelDisplayName = SwitchableServiceHelper.GetSwitchableServiceDisplayName(model.ConfigurationModel.ServiceType);
            Weight = model.Weight;
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

        private IConfigurationModel? _configuration;
        public override IConfigurationModel? Configuration
        {
            get => _configuration;
            set
            {
                if (SetProperty(ref _configuration, value))
                {
                    UpdateChildConfigurations();
                    RaisePropertyChanged(nameof(ChildConfigurations));
                }
            }
        }

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
                    RaisePropertyChanged(nameof(SelectedChildConfigurationModel));
                }
            }
        }
        
        public IConfigurationModel? SelectedChildConfigurationModel => SelectedChildConfiguration?.Model.ConfigurationModel;

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
            
            var weightedEntry = new CombinedTextComparisonConfigurationModel.WeightedConfiguration(config, 1.0f);
            ChildConfigurations.Add(new CombinedTextComparisonWeightedConfigurationViewModel(weightedEntry));
        }

        private void RemoveChildConfiguration()
        {
            if (SelectedChildConfiguration == null)
                return;
            
            ChildConfigurations.Remove(SelectedChildConfiguration);
        }
        
        public override void ApplyChanges()
        {
            base.ApplyChanges();

            foreach (var childConfiguration in ChildConfigurations)
            {
                childConfiguration.Model.Weight = childConfiguration.Weight;
            }
            
            ThisConfiguration?.ChildConfigurations.Clear();
            ThisConfiguration?.ChildConfigurations.AddRange(ChildConfigurations.Select(vm => vm.Model));
        }

        public override void Initialise()
        {
            base.Initialise();
            UpdateChildConfigurations();
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

                ChildConfigurations?.Add(new CombinedTextComparisonWeightedConfigurationViewModel(childConfiguration));
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
    }
}