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
        
        private int _wordGapThreshold;
        public int WordGapThreshold
        {
            get => _wordGapThreshold; 
            set => SetProperty(ref _wordGapThreshold, value);
        }

        private double _minimumDynamicComparisonConfidence;

        public double MinimumDynamicComparisonConfidence
        {
            get => _minimumDynamicComparisonConfidence;
            set => SetProperty(ref _minimumDynamicComparisonConfidence, value);
        }

        public override void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.SaveAnalysisTemporaryImages = SaveAnalysisTemporaryImages;
            ThisConfiguration.WordGapThreshold = WordGapThreshold;
            ThisConfiguration.MinimumDynamicComparisonConfidence = MinimumDynamicComparisonConfidence;
        }

        protected override void OnConfigurationUpdated()
        {
            if (ThisConfiguration != null)
            {
                SaveAnalysisTemporaryImages = ThisConfiguration.SaveAnalysisTemporaryImages;
                WordGapThreshold = ThisConfiguration.WordGapThreshold;
                MinimumDynamicComparisonConfidence = ThisConfiguration.MinimumDynamicComparisonConfidence;
            }
            else
            {
                SaveAnalysisTemporaryImages = false;
                WordGapThreshold = 10;
                MinimumDynamicComparisonConfidence = 0.0;
            }
        }

        public override bool CheckForChanges()
        {
            if(ThisConfiguration == null)
                return false;
            
            return SaveAnalysisTemporaryImages != ThisConfiguration.SaveAnalysisTemporaryImages ||
                   WordGapThreshold != ThisConfiguration.WordGapThreshold ||
                   Math.Abs(MinimumDynamicComparisonConfidence - ThisConfiguration.MinimumDynamicComparisonConfidence) > double.Epsilon;
        }
    }
}