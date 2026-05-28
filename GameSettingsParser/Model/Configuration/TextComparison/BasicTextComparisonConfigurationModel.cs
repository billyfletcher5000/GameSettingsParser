namespace GameSettingsParser.Model.Configuration.TextComparison
{
    public abstract class BasicTextComparisonConfigurationModel : ITextComparisonConfigurationModel
    {
        public float MinimumConfidence { get; set; } = 0.0f;
        
        public bool HasChanges { get; set; }
        
        public abstract Type ViewModelType { get; }
        public string? Section => "Text Comparison";

        public abstract Type ServiceType { get; }
        
        public abstract string DisplayName { get; }

        public abstract void ApplyChanges();
        public abstract void ResetChanges();
    }
}