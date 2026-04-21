namespace GameSettingsParser.Model.TextComparisonConfiguration
{
    public abstract class BasicTextComparisonConfigurationModel : ITextComparisonConfigurationModel
    {
        public float MinimumConfidence { get; set; } = 0.0f;
        
        public bool HasChanges { get; set; }
        
        public abstract Type ServiceType { get; }
        
        public abstract string DisplayName { get; }
    }
}