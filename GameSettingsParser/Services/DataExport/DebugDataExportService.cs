using System.IO;
using System.Text;
using System.Windows;
using GameSettingsParser.Model;

namespace GameSettingsParser.Services.DataExport
{
    public class DebugDataExportService : IDataExportService
    {
        public bool SupportsExportToFile => true;
        public bool SupportsExportToClipboard => true;
        public string FileExtension => ".txt";
        public string FileFilter => "Text Files (*.txt)|*.txt";

        public void ExportToClipboard(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            var output = CreateDebugOutput(imageAnalysisResult, parsingProfile);
            Clipboard.SetText(output);
        }

        public void ExportToFile(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath)
        {
            var output = CreateDebugOutput(imageAnalysisResult, parsingProfile);
            File.WriteAllText(outputPath, output);
        }
        
        private string CreateDebugOutput(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            StringBuilder sb = new();

            sb.AppendLine("Image Results:");
            foreach (var setting in imageAnalysisResult.ProcessedImages)
            {
                sb.AppendLine($"\tScreenshot: {setting.ScreenshotPath}");
                foreach (var (markupType, values) in setting.MarkupTypeToValues)
                {
                    sb.AppendLine($"\t\tMarkup Type: {markupType} Values: {string.Join(", ", values)}");
                }
            }
            
            return sb.ToString();
        }
    }
}