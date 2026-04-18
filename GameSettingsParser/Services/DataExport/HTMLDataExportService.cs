using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using GameSettingsParser.Model;
using GameSettingsParser.Utility;

namespace GameSettingsParser.Services.DataExport
{
    public class HTMLDataExportService : IDataExportService
    {
        public bool SupportsExportToFile => true;
        public bool SupportsExportToClipboard => true;
        
        public string FileExtension => ".html";
        public string FileFilter => "HTML Files (*.html)|*.html";

        public void ExportToClipboard(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            var output = ExportToHTMLString(imageAnalysisResult, parsingProfile, true);
            Clipboard.SetText(output);
        }
        
        public void ExportToFile(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath)
        {
            var export = ExportToHTMLString(imageAnalysisResult, parsingProfile, false);
            var output = $"<html>\n\t<head>\n\t\t<title>Game Settings Parser Export</title>\n\t</head>\n\t<body>\n\n{export}\n\n\t</body>\n</html>";
            File.WriteAllText(outputPath, output);
        }

        private string ExportToHTMLString(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, bool screenshotsAsBase64)
        {
            StringBuilder sb = new();

            int indent = 0;
            var organisedSettings = SectionDataTableHelper.ConvertAnalysisResult(imageAnalysisResult, parsingProfile);
            foreach (var section in organisedSettings)
            {
                var columns = section.Settings.Columns.Cast<DataColumn>().ToList();
                sb.AppendLine("<div>");
                sb.AppendLine($"<h2>{section.Name}</h2>");
                sb.AppendLine("<table>"); 
                indent++;
                
                sb.AppendLine($"{new string('\t', indent)}<tr>"); 
                indent++;
                foreach (var column in columns)
                {
                    sb.AppendLine($"{new string('\t', indent)}<th>{column.ColumnName}</th>");
                } 
                --indent;
                
                sb.AppendLine($"{new string('\t', indent)}</tr>"); 
                
                foreach (var row in section.Settings.Rows.Cast<DataRow>())
                {
                    sb.AppendLine($"{new string('\t', indent)}<tr>");
                    indent++;
                    foreach (var column in columns)
                    {
                        string? cellValue = row[column] as string;
                        if (cellValue == null)
                            continue;
                        
                        sb.AppendLine($"{new string('\t', indent)}<td>");
                        indent++;
                        if (column.ColumnName == SectionDataTableHelper.ScreenshotColumnName)
                        {
                            
                            var screenshotPaths = cellValue.Split(SectionDataTableHelper.ListSeparator);
                            foreach (var screenshotPath in screenshotPaths)
                            {
                                if (screenshotsAsBase64)
                                {
                                    var bytes = File.ReadAllBytes(screenshotPath);
                                    var base64 = Convert.ToBase64String(bytes);
                                    var mimeType = GetMIMETypeFromFile(screenshotPath);
                                    sb.AppendLine($"{new string('\t', indent)}<img src=\"data:{mimeType};base64,{base64}\" width=\"96\" height=\"54\" />\n");
                                }
                                else
                                {
                                    var relativePath = Path.GetFileName(screenshotPath);
                                    sb.AppendLine($"{new string('\t', indent)}<a href=\"{relativePath}\"><img src=\"{relativePath}\" width=\"96\" height=\"54\" /></a>\n");
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine($"{new string('\t', indent)}{cellValue}");
                        }
                        --indent;
                        sb.AppendLine($"{new string('\t', indent)}</td>");
                    }
                    --indent;
                    sb.AppendLine($"{new string('\t', indent)}<tr>");
                }
                
                --indent;
                sb.AppendLine("</table>");
                sb.AppendLine("</div>");
            }
            
            return sb.ToString();
        }

        private string GetMIMETypeFromFile(string imagePath)
        {
            return Path.GetExtension(imagePath).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".tif" or ".tiff" => "image/tiff",
                _ => "application/octet-stream"
            };
        }
    }
}