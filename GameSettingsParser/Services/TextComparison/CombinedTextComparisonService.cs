using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using GameSettingsParser.Settings;

namespace GameSettingsParser.Services.TextComparison
{
    public class CombinedTextComparisonService : ITextComparisonService
    {
        private readonly List<ITextComparisonService> _services = [];
        
        public CombinedTextComparisonService()
        {
            var serviceTypes = UserSettings.Instance.TextComparisonServices;
            serviceTypes.CollectionChanged += OnServiceTypesSettingsCollectionChanged;
            
            // We instantiate each type every time
            foreach (var serviceType in serviceTypes)
                InstantiateServiceType(serviceType);
        }

        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB)
        {
            // TODO: Add weighting
            double aggregate = 0.0f;
            foreach (var service in _services)
            {
                aggregate += service.GetConfidenceInterval(imageA, imageB);
            }
            
            return aggregate / _services.Count;
        }

        private void InstantiateServiceType(Type serviceType)
        {
            object? serviceInstance = Activator.CreateInstance(serviceType);
            if (serviceInstance is not ITextComparisonService textComparisonService)
            {
                Console.WriteLine($"Failed to create instance of {serviceType.Name}, likely not a valid {nameof(ITextComparisonService)}");
                return;
            }
                
            _services.Add(textComparisonService);
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