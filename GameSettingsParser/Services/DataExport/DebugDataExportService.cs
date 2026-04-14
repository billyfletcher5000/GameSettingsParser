using System.IO;
using System.Text;
using GameSettingsParser.Model;

namespace GameSettingsParser.Services.DataExport
{
    public class DebugDataExportService : IDataExportService
    {
        public bool ExportsToFile => true;
        
        public void Export(ImageAnalysisResultModel imageAnalysisResult, string? outputPath)
        {
            StringBuilder sb = new();

            sb.AppendLine("Image Results:");
            foreach (var setting in imageAnalysisResult.Settings)
            {
                sb.AppendLine($"\tScreenshot: {setting.ScreenshotPath}");
                foreach (var (markupType, values) in setting.MarkupTypeToValues)
                {
                    sb.AppendLine($"\t\tMarkup Type: {markupType} Values: {string.Join(", ", values)}");
                }
            }
            
            var output = sb.ToString();
            Console.WriteLine(output);

            if (outputPath != null)
            {
                File.WriteAllText(outputPath, output);
            }
        }
    }
}