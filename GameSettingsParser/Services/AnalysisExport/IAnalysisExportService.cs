using GameSettingsParser.Model;

namespace GameSettingsParser.Services.AnalysisExport
{
    public interface IAnalysisExportService
    {
        public bool SupportsExportToFile => false;
        public bool SupportsExportToClipboard => false;
        public bool SupportsExportToWebsite => false;
        
        public string FileExtension => string.Empty;
        public string FileFilter => string.Empty;
        
        public Task ExportToClipboardAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage)
        {
            throw new NotImplementedException();
        }
        
        public Task ExportToFileAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath, CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage)
        {
            throw new NotImplementedException();
        }
        
        public Task ExportToWebsiteAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage)
        {
            throw new NotImplementedException();
        }
    }
}