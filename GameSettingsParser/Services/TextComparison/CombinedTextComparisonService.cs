using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using GameSettingsParser.Attributes;
using GameSettingsParser.Controls.TextComparison;
using GameSettingsParser.Model;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.TextComparison;

namespace GameSettingsParser.Services.TextComparison
{
    [SwitchableService(nameof(CombinedTextComparisonConfigurationModel), "Combined")]
    public class CombinedTextComparisonService : ITextComparisonService
    {
        private readonly List<ITextComparisonService> _services = [];

        private CombinedTextComparisonConfigurationModel? _thisConfiguration;

        public IConfigurationModel? Configuration
        {
            get => ThisConfiguration;
            set => ThisConfiguration = value as CombinedTextComparisonConfigurationModel;
        }

        public CombinedTextComparisonConfigurationModel? ThisConfiguration
        {
            get => _thisConfiguration;
            set
            {
                if (_thisConfiguration == value) return;
                
                if (_thisConfiguration != null)
                    _thisConfiguration.Configurations.CollectionChanged -= OnServiceTypesSettingsCollectionChanged;
                
                _thisConfiguration = value;
                _services.Clear();
                
                if (_thisConfiguration != null)
                {
                    var configurations = _thisConfiguration.Configurations;
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

        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB, ParsingProfileModel parsingProfile)
        {
            // TODO: Add weighting
            double aggregate = 0.0f;
            foreach (var service in _services)
            {
                var weight = ThisConfiguration?.Configurations.FirstOrDefault(c => c.ConfigurationModel.ServiceType == service.GetType()).Weight ?? 1.0f;
                aggregate += service.GetConfidenceInterval(imageA, imageB, parsingProfile) * weight;
            }
            
            var confidence = aggregate / _services.Count;
            
            if(confidence < ThisConfiguration?.MinimumConfidence)
                return 0.0;
            
            return confidence;
        }

        private ITextComparisonService? InstantiateServiceType(Type serviceType)
        {
            object? serviceInstance = Activator.CreateInstance(serviceType);
            if (serviceInstance is not ITextComparisonService textComparisonService)
            {
                Console.WriteLine($"Failed to create instance of {serviceType.Name}, likely not a valid {nameof(ITextComparisonService)}");
                return null;
            }
                
            _services.Add(textComparisonService);
            return textComparisonService;
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