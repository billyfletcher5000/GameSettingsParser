using GameSettingsParser.Model;

namespace GameSettingsParser.Services.DataExport
{
    public interface IDataExportService
    {
        public bool SupportsExportToFile { get; }
        public bool SupportsExportToClipboard { get; }
        
        public string FileExtension { get; }
        public string FileFilter { get; }
        
        public void ExportToClipboard(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile);
        public void ExportToFile(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath);
    }
}