using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public interface IConfigurationViewModel : IConfigurationTreeViewItem
    {
        public Type ViewType { get; }
        
        public IConfigurationModel? Configuration { get; set; }

        public void ApplyChanges();
        
        // Reset changes should be called after configuration is set during creation
        public void ResetChanges();
    }
}