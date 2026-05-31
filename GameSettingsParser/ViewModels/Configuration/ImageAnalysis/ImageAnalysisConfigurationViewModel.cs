using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.ImageAnalysis;
using GameSettingsParser.Views.Configuration.ImageAnalysis;

namespace GameSettingsParser.ViewModels.Configuration.ImageAnalysis
{
    public class ImageAnalysisConfigurationViewModel : BindableBase, IConfigurationViewModel
    {
        public Type ViewType => typeof(ImageAnalysisConfigurationView);
        
        public string DisplayName => ThisConfiguration?.DisplayName ?? "Image Analysis";
        
        public IConfigurationModel? Configuration { get; set; }
        public ImageAnalysisConfigurationModel? ThisConfiguration => Configuration as ImageAnalysisConfigurationModel;
    
        private bool _saveAnalysisTemporaryImages;
        public bool SaveAnalysisTemporaryImages
        {
            get => _saveAnalysisTemporaryImages; 
            set => SetProperty(ref _saveAnalysisTemporaryImages, value);
        }

        public void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.SaveAnalysisTemporaryImages = SaveAnalysisTemporaryImages;
        }

        public void ResetChanges()
        {
            if (ThisConfiguration != null)
            {
                SaveAnalysisTemporaryImages = ThisConfiguration.SaveAnalysisTemporaryImages;
            }
            else
            {
                SaveAnalysisTemporaryImages = false;
            }
        }

        public bool CheckForChanges()
        {
            if(ThisConfiguration == null)
                return false;
            
            return SaveAnalysisTemporaryImages != ThisConfiguration.SaveAnalysisTemporaryImages;
        }
    }
}