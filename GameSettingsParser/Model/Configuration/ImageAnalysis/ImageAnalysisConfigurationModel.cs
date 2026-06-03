using GameSettingsParser.Constants;
using GameSettingsParser.ViewModels.Configuration.ImageAnalysis;

namespace GameSettingsParser.Model.Configuration.ImageAnalysis
{
    public abstract class ImageAnalysisConfigurationModel : IConfigurationModel
    {
        public abstract string DisplayName { get; }
        public Type ViewModelType => typeof(ImageAnalysisConfigurationViewModel);
        public string? Section => $"{ConfigurationSectionConstants.Project}/{ConfigurationSectionConstants.ImageAnalysis}";
    
        public bool SaveAnalysisTemporaryImages { get; set; } = false;
        
        /// <summary>
        /// The amount of pixels between words' bounding boxes for them to be considered part of the same text string
        /// </summary>
        public int WordGapThreshold { get; set; } = 10;

        public double MinimumDynamicComparisonConfidence { get; set; }
    }
}