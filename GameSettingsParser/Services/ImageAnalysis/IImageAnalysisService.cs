using GameSettingsParser.Model;

namespace GameSettingsParser.Services.ImageAnalysis
{
    public interface IImageAnalysisService
    {
        Task<ImageAnalysisResultModel?> AnalyseAsync(ParsingProfileModel parsingProfile, string[] imagePathsToAnalyse,
            CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage);
    }
}