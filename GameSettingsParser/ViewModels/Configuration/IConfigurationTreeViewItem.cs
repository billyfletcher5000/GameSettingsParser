namespace GameSettingsParser.ViewModels.Configuration
{
    public interface IConfigurationTreeViewItem
    {
        public string DisplayName { get; }
        
        public void ApplyChanges();
        
        public bool CheckForChanges();
    }
}