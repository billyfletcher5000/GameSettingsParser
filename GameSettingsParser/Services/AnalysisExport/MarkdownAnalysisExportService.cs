using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using GameSettingsParser.Attributes;
using GameSettingsParser.Model;
using GameSettingsParser.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Size = SixLabors.ImageSharp.Size;

namespace GameSettingsParser.Services.AnalysisExport
{
    [SwitchableService(nameof(MarkdownAnalysisExportService), "Markdown")]
    public class MarkdownAnalysisExportService : IAnalysisExportService
    {
        public bool SupportsExportToFile => true;
        public bool SupportsExportToClipboard => true;
        public string FileExtension => ".md";
        public string FileFilter => "Markdown Files (*.md)|*.md";
        
        public Size ThumbnailSize { get; set; } = new Size(96, 54);
        
        public void ExportToClipboardAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            var markdownOutput = CreateMarkdownOutput(imageAnalysisResult, parsingProfile);
            Clipboard.SetText(markdownOutput);
        }

        public void ExportToFileAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, string outputPath)
        {
            var markdownOutput = CreateMarkdownOutput(imageAnalysisResult, parsingProfile, true, outputPath);
            File.WriteAllText(outputPath, markdownOutput);
        }

        private string CreateMarkdownOutput(ImageAnalysisResultModel imageAnalysisResult,
            ParsingProfileModel parsingProfile, bool copyScreenshotsAndCreateThumbnails = false, string outputPath = "")
        {
            var screenshotsFolder = !string.IsNullOrEmpty(outputPath) ? Path.Combine(Path.GetDirectoryName(outputPath)!, "screenshots") : string.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"# Game Settings - {parsingProfile.Name}");

            var sectionData = SectionDataTableHelper.ConvertAnalysisResult(imageAnalysisResult, parsingProfile);
            foreach (var section in sectionData)
            {
                sb.AppendLine();
                sb.AppendLine($"## {section.Name}");
                sb.AppendLine();
                
                foreach (DataColumn column in section.Settings.Columns)
                    sb.Append('|').Append(column.ColumnName.Trim());

                sb.AppendLine("|");

                for (int i = 0; i < section.Settings.Columns.Count; i++)
                    sb.Append("|----");

                sb.AppendLine("|");

                foreach (var row in section.Settings.Rows.Cast<DataRow>())
                {
                    sb.Append('|');
                    
                    foreach (DataColumn column in section.Settings.Columns)
                    {
                        if (row[column] is not string cellValue)
                            continue;

                        if (column.ColumnName == SectionDataTableHelper.ScreenshotColumnName)
                        {
                            var screenshotPaths = cellValue.Split(SectionDataTableHelper.ListSeparator);
                            foreach (var screenshotPath in screenshotPaths)
                            {
                                var finalPath = screenshotPath;
                                var thumbnailPath = finalPath;
                                
                                if (copyScreenshotsAndCreateThumbnails && !string.IsNullOrEmpty(screenshotsFolder))
                                {
                                    finalPath = Path.Combine(Path.Combine(screenshotsFolder, section.Name.ToLower()),
                                        Path.GetFileName(screenshotPath));
                                    Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
                                    
                                    thumbnailPath = Path.GetDirectoryName(finalPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(finalPath) + "_th.png";  
                                    
                                    var thumbnail = Image.Load(screenshotPath);
                                    thumbnail.Mutate(x => x.Resize(ThumbnailSize.Width, ThumbnailSize.Height));
                                    thumbnail.Save(thumbnailPath);
                                    
                                    File.Copy(screenshotPath, finalPath, true);
                                    
                                    
                                    finalPath = Path.GetRelativePath(Path.GetDirectoryName(outputPath)!, finalPath);
                                    thumbnailPath = Path.GetRelativePath(Path.GetDirectoryName(outputPath)!, thumbnailPath);
                                }

                                sb.Append($"[![{Path.GetFileNameWithoutExtension(finalPath)}]({thumbnailPath} \"{Path.GetFileNameWithoutExtension(finalPath)}\")]({finalPath})");
                            }
                        }
                        else
                        {
                            sb.Append($"{cellValue}");
                        }

                        sb.Append('|');
                    }
                    
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}