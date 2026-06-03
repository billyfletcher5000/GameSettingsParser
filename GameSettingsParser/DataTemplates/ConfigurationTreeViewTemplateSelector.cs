using System.Windows;
using System.Windows.Controls;
using GameSettingsParser.ViewModels.Configuration;

namespace GameSettingsParser.DataTemplates
{
    public class ConfigurationTreeViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? SectionTemplate { get; set; }
        public DataTemplate? ConfigTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is null)
                return null;
            
            if (item is ConfigurationSectionViewModel)
                return SectionTemplate;
            
            if (item is ConfigurationViewModelBase)
                return ConfigTemplate;
            
            return base.SelectTemplate(item, container);
        }
    }
}