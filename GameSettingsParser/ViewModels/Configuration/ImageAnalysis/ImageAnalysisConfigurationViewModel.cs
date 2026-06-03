using GameSettingsParser.Model.Configuration.ImageAnalysis;
using GameSettingsParser.Views.Configuration.ImageAnalysis;

namespace GameSettingsParser.ViewModels.Configuration.ImageAnalysis
{
    public class ImageAnalysisConfigurationViewModel : ConfigurationViewModelBase
    {
        public override Type ViewType => typeof(ImageAnalysisConfigurationView);
        
        public override string DisplayName => ThisConfiguration?.DisplayName ?? "Image Analysis";
        
        public ImageAnalysisConfigurationModel? ThisConfiguration => Configuration as ImageAnalysisConfigurationModel;
    
        private bool _saveAnalysisTemporaryImages;
        public bool SaveAnalysisTemporaryImages
        {
            get => _saveAnalysisTemporaryImages; 
            set => SetProperty(ref _saveAnalysisTemporaryImages, value);
        }

        public override void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.SaveAnalysisTemporaryImages = SaveAnalysisTemporaryImages;
        }

        protected override void OnConfigurationUpdated()
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

        public override bool CheckForChanges()
        {
            if(ThisConfiguration == null)
                return false;
            
            return SaveAnalysisTemporaryImages != ThisConfiguration.SaveAnalysisTemporaryImages;
        }
    }
}