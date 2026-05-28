using GameSettingsParser.Services.AnalysisExport;

namespace GameSettingsParser.ServiceProviders.AnalysisExport
{
    public interface IAnalysisExportServiceProvider
    {
        public IAnalysisExportService Current { get; }
        public event Action<IAnalysisExportService>? CurrentChanged;
    }
}