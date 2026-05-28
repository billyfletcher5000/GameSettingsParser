using System.Windows;
using System.Windows.Controls;
using GameSettingsParser.ViewModels.Configuration;

namespace GameSettingsParser.DataTemplates
{
    public class ConfigurationTemplateSelector : DataTemplateSelector
    {
        public static readonly ConfigurationTemplateSelector Instance = new();
        
        private readonly Dictionary<Type, DataTemplate> _templateCache = new();
        
        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if(item is not IConfigurationViewModel configurationModel)
                return base.SelectTemplate(item, container);

            if (!_templateCache.TryGetValue(configurationModel.GetType(), out var template))
            {
                template = new DataTemplate()
                {
                    DataType = configurationModel.GetType(),
                    VisualTree = new FrameworkElementFactory(configurationModel.ViewType)
                };
                
                _templateCache.Add(configurationModel.GetType(), template);
            }
            
            return template;
        }
    }
}