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
        
        public void ExportToClipboard(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            throw new NotImplementedException();
        }
        
        public void ExportToFile(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath)
        {
            throw new NotImplementedException();
        }
        
        public Task ExportToWebsiteAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            throw new NotImplementedException();
        }
    }
}