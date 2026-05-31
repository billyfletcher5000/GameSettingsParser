using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using GameSettingsParser.Attributes;
using GameSettingsParser.Controls.TextComparison;
using GameSettingsParser.Model;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Services.Configuration;
using GameSettingsParser.Services.Logging;
using GameSettingsParser.Utility;

namespace GameSettingsParser.Services.TextComparison
{
    [SwitchableService(nameof(CombinedTextComparisonService), "Combined")]
    public class CombinedTextComparisonService : ITextComparisonService
    {
        private readonly List<ITextComparisonService> _services = [];
        private readonly IContainerProvider _containerProvider;
        private readonly ILogService _log;

        private CombinedTextComparisonConfigurationModel? _thisConfiguration;

        public IConfigurationModel? Configuration
        {
            get => ThisConfiguration;
            set => ThisConfiguration = value as CombinedTextComparisonConfigurationModel;
        }

        public Type ConfigurationType => typeof(CombinedTextComparisonConfigurationModel);

        public CombinedTextComparisonConfigurationModel? ThisConfiguration
        {
            get => _thisConfiguration;
            set
            {
                if (_thisConfiguration == value) return;
                
                if (_thisConfiguration != null)
                    _thisConfiguration.ChildConfigurations.CollectionChanged -= OnServiceTypesSettingsCollectionChanged;
                
                _thisConfiguration = value;
                _services.Clear();
                
                if (_thisConfiguration != null)
                {
                    var configurations = _thisConfiguration.ChildConfigurations;
                    configurations.CollectionChanged += OnServiceTypesSettingsCollectionChanged;
            
                    // We instantiate each type every time
                    foreach (var configuration in configurations)
                    {
                        var service = InstantiateServiceType(configuration.ConfigurationModel.ServiceType);
                        if (service is not null)
                            service.Configuration = configuration.ConfigurationModel;
                    }
                }
            }
        }

        public CombinedTextComparisonService(IConfigurationService configurationService, IContainerProvider containerProvider, ILogService logService)
        {
            var configuration = configurationService.GetConfiguration<CombinedTextComparisonConfigurationModel>();
            if (configuration != null)
                ThisConfiguration = configuration;
            
            _containerProvider = containerProvider;
            _log = logService;
        }
        
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB, ParsingProfileModel parsingProfile)
        {
            // TODO: Add weighting
            double aggregate = 0.0f;
            foreach (var service in _services)
            {
                var weight = ThisConfiguration?.ChildConfigurations.FirstOrDefault(c => c.ConfigurationModel.ServiceType == service.GetType()).Weight ?? 1.0f;
                aggregate += service.GetConfidenceInterval(imageA, imageB, parsingProfile) * weight;
            }
            
            var confidence = aggregate / _services.Count;
            
            if (confidence < ThisConfiguration?.MinimumConfidence)
                return 0.0;
            
            return confidence;
        }

        private ITextComparisonService? InstantiateServiceType(Type serviceType)
        {
            var serviceId = SwitchableServiceHelper.GetSwitchableServiceId(serviceType);
            if (serviceId == null)
            {
                _log.Error($"Failed to get service ID for {serviceType.Name}");
                return null;
            }

            var serviceInstance = _containerProvider.Resolve<ITextComparisonService>(serviceId);
            if (serviceInstance is null)
            {
                _log.Error($"Failed to create instance of {serviceType.Name}, likely not a valid {nameof(ITextComparisonService)}");
                return null;
            }
                
            _services.Add(serviceInstance);
            return serviceInstance;
        }

        private void OnServiceTypesSettingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddAll(e.NewItems);
                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    RemoveAll(e.OldItems);
                    break;
                
                case NotifyCollectionChangedAction.Replace:
                    RemoveAll(e.OldItems);
                    AddAll(e.NewItems);
                    break;
                
                case NotifyCollectionChangedAction.Reset:
                    _services.Clear();
                    break;
            }
        }
        
        private void AddAll(IList? items)
        {
            if (items != null)
            {
                foreach (var item in items)
                    InstantiateServiceType((Type)item);
            }
        }

        private void RemoveAll(IList? items)
        {
            if (items != null)
            {
                foreach (var item in items)
                    _services.RemoveAll(service => service.GetType() == (Type)item);
            }
        }
    }
}