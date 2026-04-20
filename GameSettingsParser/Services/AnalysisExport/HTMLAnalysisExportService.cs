using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using GameSettingsParser.Model;
using GameSettingsParser.Utility;

namespace GameSettingsParser.Services.AnalysisExport
{
    public class HTMLAnalysisExportService : IAnalysisExportService
    {
        private const string StylesheetPath = "html_export/styles.css";
        
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
            var result = MessageBox.Show("Copy images to same folder as HTML file?", "Copy Images", MessageBoxButton.YesNo);
            var export = ExportToHTMLString(imageAnalysisResult, parsingProfile, false, result == MessageBoxResult.Yes, outputPath);
            var output = $"{CreateHTMLHeader()}<body>\n\n{export}\n\n\t</body>\n</html>";
            File.WriteAllText(outputPath, output);
            File.Copy(StylesheetPath, Path.Combine(Path.GetDirectoryName(outputPath)!, Path.GetFileName(StylesheetPath)), true);
        }

        private string CreateHTMLHeader()
        {
            return "<!DOCTYPE html>\n" +
                   "<html>\n" +
                   "\n\t<head>" +
                   "\n\t\t<meta charset=\"utf-8\">" +
                   "\n\t\t<link rel=\"stylesheet\" href=\"styles.css\">" +
                   "\n\t\t<title>Game Settings Parser Export</title>" +
                   "\n\t</head>\n\t";
        }

        private string ExportToHTMLString(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, bool screenshotsAsBase64, bool copyScreenshots = false, string outputPath = "")
        {
            var screenshotsFolder = !string.IsNullOrEmpty(outputPath) ? Path.Combine(Path.GetDirectoryName(outputPath)!, "screenshots") : string.Empty;
            
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
                
                
                sb.AppendLine($"{new string('\t', indent)}<thead>"); 
                indent++;
                sb.AppendLine($"{new string('\t', indent)}<tr>"); 
                indent++;
                foreach (var column in columns)
                {
                    var markupType = parsingProfile.MarkupTypes.FirstOrDefault(item => item.Name.Equals(column.ColumnName));
                    var columnWidth = markupType != null ? markupType.HTMLExportTableWidth : string.Empty;
                    if(!string.IsNullOrEmpty(columnWidth))
                        sb.AppendLine($"{new string('\t', indent)}<th width=\"{columnWidth}\">{column.ColumnName}</th>");
                    else
                        sb.AppendLine($"{new string('\t', indent)}<th>{column.ColumnName}</th>");
                } 
                --indent;
                sb.AppendLine($"{new string('\t', indent)}</tr>");
                --indent;
                sb.AppendLine($"{new string('\t', indent)}</thead>"); 
                
                
                sb.AppendLine($"{new string('\t', indent)}<tbody>"); 
                indent++;
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
                                    var finalPath = screenshotPath;
                                    if (copyScreenshots && !string.IsNullOrEmpty(screenshotsFolder))
                                    {
                                        finalPath = Path.Combine(Path.Combine(screenshotsFolder, section.Name.ToLower()), Path.GetFileName(screenshotPath));
                                        Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
                                        File.Copy(screenshotPath, finalPath, true);
                                        finalPath = Path.GetRelativePath(Path.GetDirectoryName(outputPath)!, finalPath);
                                    }

                                    sb.AppendLine($"{new string('\t', indent)}<a href=\"{finalPath}\"><img src=\"{finalPath}\" width=\"96\" height=\"54\" /></a>\n");
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
                    sb.AppendLine($"{new string('\t', indent)}</tr>");
                }
                --indent;
                sb.AppendLine($"{new string('\t', indent)}</tbody>"); 
                
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