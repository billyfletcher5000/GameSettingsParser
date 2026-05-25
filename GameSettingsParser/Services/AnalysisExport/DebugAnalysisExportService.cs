using System.IO;
using System.Text;
using System.Windows;
using GameSettingsParser.Model;

namespace GameSettingsParser.Services.AnalysisExport
{
    public class DebugAnalysisExportService : IAnalysisExportService
    {
        public bool SupportsExportToFile => true;
        public bool SupportsExportToClipboard => true;
        public string FileExtension => ".txt";
        public string FileFilter => "Text Files (*.txt)|*.txt";

        public Task ExportToClipboardAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage)
        {
            var output = CreateDebugOutput(imageAnalysisResult, parsingProfile);
            Clipboard.SetText(output);
            return Task.CompletedTask;
        }

        public Task ExportToFileAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath, CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage)
        {
            var output = CreateDebugOutput(imageAnalysisResult, parsingProfile);
            File.WriteAllText(outputPath, output);
            return Task.CompletedTask;
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