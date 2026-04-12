using GameSettingsParser.Model;

namespace GameSettingsParser.Services.DataExport
{
    public class ConfluenceDataExportService : IDataExportService
    {
        public bool ExportsToFile => true;
        
        public void Export(ImageAnalysisResultModel imageAnalysisResult, string? outputPath)
        {
        }
    }
}