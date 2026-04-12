using GameSettingsParser.Model;

namespace GameSettingsParser.Services.DataExport
{
    public interface IDataExportService
    {
        public void Export(ImageAnalysisResultModel imageAnalysisResult, string? outputPath);

        public bool ExportsToFile { get; }
    }
}