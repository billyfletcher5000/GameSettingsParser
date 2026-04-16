using System.IO;
using System.Text;
using System.Windows;
using GameSettingsParser.Model;

namespace GameSettingsParser.Services.DataExport
{
    public class ConfluenceDataExportService : IDataExportService
    {
        public bool ExportsToFile => true;
        
        public void Export(ImageAnalysisResultModel imageAnalysisResult, string? outputPath)
        {
            StringBuilder sb = new();

            foreach (var setting in imageAnalysisResult.Settings)
            {
                
            }
            
            var output = sb.ToString();
            
            Clipboard.SetText(output);
            
            if(outputPath != null)
                File.WriteAllText(outputPath, output);
        }
    }
}