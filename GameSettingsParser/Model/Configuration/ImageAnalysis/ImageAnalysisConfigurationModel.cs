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
    }
}