using System.Collections.ObjectModel;
using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public class ConfigurationSectionViewModel : IConfigurationTreeViewItem
    {
        public required string DisplayName { get; set; }

        public ObservableCollection<IConfigurationTreeViewItem> TreeViewItems { get; set; } = new();
        
        public bool CheckForChanges()
        {
            foreach (var treeViewItem in TreeViewItems)
            {
                if(treeViewItem.CheckForChanges())
                    return true;
            }

            return false;
        }
    }
}