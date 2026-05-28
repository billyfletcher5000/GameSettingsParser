namespace GameSettingsParser.Model.Configuration
{
    public interface IConfigurationModel
    {
        public bool HasChanges { get; set; }
        public string DisplayName { get; }
        public Type ViewModelType { get; }
        public string? Section { get; }
        
        public void ApplyChanges();
        
        public void ResetChanges();
    }
}