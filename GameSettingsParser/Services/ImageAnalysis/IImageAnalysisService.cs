using GameSettingsParser.Model;

namespace GameSettingsParser.Services.ImageAnalysis
{
    public interface IImageAnalysisService
    {
        ImageAnalysisResultModel? Analyse(ParsingProfileModel parsingProfile, string[] imagePathsToAnalyse);
    }
    
    
}