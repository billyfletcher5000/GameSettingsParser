using GameSettingsParser.Model;

namespace GameSettingsParser.Services
{
    public interface IDataExportService
    {
        public void Export(ImageAnalysisResultModel imageAnalysisResult, string? outputPath);

        public bool ExportsToFile { get; }
    }
    
    public class ConfluenceDataExportService : IDataExportService
    {
        public bool ExportsToFile => true;
        
        public void Export(ImageAnalysisResultModel imageAnalysisResult, string? outputPath)
        {
            
        }
    }
}