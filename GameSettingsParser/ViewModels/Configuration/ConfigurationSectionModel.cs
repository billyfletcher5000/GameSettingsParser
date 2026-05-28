using System.Collections.ObjectModel;
using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public class ConfigurationSectionViewModel
    {
        public required string DisplayName { get; set; }
        public ObservableCollection<IConfigurationViewModel> Configurations { get; set; } = new();
    }
}