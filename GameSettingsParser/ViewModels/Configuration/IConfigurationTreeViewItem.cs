namespace GameSettingsParser.ViewModels.Configuration
{
    public interface IConfigurationTreeViewItem
    {
        public string DisplayName { get; }
        
        public bool CheckForChanges();
    }
}