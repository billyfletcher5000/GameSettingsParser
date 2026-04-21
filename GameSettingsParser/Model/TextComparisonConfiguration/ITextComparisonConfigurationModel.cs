namespace GameSettingsParser.Model.TextComparisonConfiguration
{
    public interface ITextComparisonConfigurationModel
    {
        public bool HasChanges { get; set; }
        
        public Type ServiceType { get; }
        
        public string DisplayName { get; }
    }
}