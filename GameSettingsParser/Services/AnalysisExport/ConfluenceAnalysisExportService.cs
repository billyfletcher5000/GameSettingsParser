using System.Data;
using System.IO;
using System.Text;
using GameSettingsParser.Model;
using GameSettingsParser.Services.Authentication;
using GameSettingsParser.Services.Confluence;
using GameSettingsParser.Services.KeyVault;
using GameSettingsParser.Utility;
using GameSettingsParser.ViewModels;
using GameSettingsParser.Views;

namespace GameSettingsParser.Services.AnalysisExport
{
    public class ConfluenceAnalysisExportService : IAnalysisExportService
    {
        private const string Audience = "api.atlassian.com";
        private const string ClientId = "vniU4R2yLFKcQ8VMsFtO5hahMGT2Io5h";
        private const string ClientSecretKeyId = "Atlassian";
        private const string Scope = "read:page:confluence write:page:confluence read:space:confluence write:confluence-file";
        private const string AuthorizationEndpoint = "https://auth.atlassian.com/authorize";
        private const string TokenEndpoint = "https://auth.atlassian.com/oauth/token";
        
        public bool SupportsExportToWebsite => true;

        private readonly IAuthenticationService _authenticationService;
        private readonly IKeyVaultService _keyVaultService;
        private readonly ConfluenceApiService _confluenceApiService;
        
        public ConfluenceAnalysisExportService(IAuthenticationService authenticationService, IKeyVaultService keyVaultService, ConfluenceApiService confluenceApiService)
        {
           _authenticationService = authenticationService;
           _keyVaultService = keyVaultService;
           _confluenceApiService = confluenceApiService;
        }
        
        public async Task ExportToWebsiteAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            var authenticationOptions = new AuthenticationOptions()
            {
                Audience = Audience,
                AuthorizationEndpoint = AuthorizationEndpoint,
                ClientId = ClientId,
                ClientSecret = _keyVaultService.GetClientSecret(ClientSecretKeyId)!,
                Scope = Scope,
                TokenEndpoint = TokenEndpoint,
            };

            var accessToken = await _authenticationService.AuthenticateAsync(authenticationOptions);
            
            // Should this be done in a service? It seems anti-pattern as it's view related but also a very useful way to do things
            ConfluenceExportDialogViewModel dialogViewModel = new(_confluenceApiService, accessToken);
            ConfluenceExportDialog dialog = new ConfluenceExportDialog(dialogViewModel);

            if (dialog.ShowDialog() != true)
                return;

            var exportConfig = dialogViewModel.Config;
            CancellationToken cancellationToken = CancellationToken.None;
            
            Dictionary<string, ConfluenceAttachment> imagePathToAttachment = new();
            
            // Upload image attachments
            foreach (var processedImage in imageAnalysisResult.ProcessedImages)
            {
                var imagePath = processedImage.ScreenshotPath;
                var contentType = ImageContentTypeHelper.GetImageContentType(imagePath);
                
                await using var attachmentStream = File.OpenRead(imagePath);
                
                var uploadedAttachment = await _confluenceApiService.UploadAttachmentToPageAsync(
                    accessToken,
                    exportConfig.SiteId,
                    exportConfig.PageId,
                    attachmentStream,
                    Path.GetFileName(processedImage.ScreenshotPath),
                    contentType,
                    cancellationToken);

                if (uploadedAttachment != null)
                    imagePathToAttachment.Add(imagePath, uploadedAttachment);
            }
            
            var content = GenerateConfluenceStorageFormatTableFromAnalysisResult(imageAnalysisResult, parsingProfile, imagePathToAttachment);
            
            var pageContent = await _confluenceApiService.GetPageStorageBodyAsync(accessToken, exportConfig.SiteId, exportConfig.PageId, cancellationToken);

            switch (exportConfig.Mode)
            {
                case ConfluenceExportConfigMode.Overwrite:
                    pageContent = content;
                    break;
                
                default:
                case ConfluenceExportConfigMode.Append:
                    pageContent += content;
                    break;
            }
            
            await _confluenceApiService.UpdatePageStorageBodyAsync(accessToken, exportConfig.SiteId, exportConfig.PageId, exportConfig.PageTitle, exportConfig.PageVersion, pageContent, cancellationToken);
        }

        private static string GenerateConfluenceStorageFormatTableFromAnalysisResult(ImageAnalysisResultModel imageAnalysisResult,
            ParsingProfileModel parsingProfile, Dictionary<string, ConfluenceAttachment> imagePathToAttachment)
        {
            StringBuilder sb = new();

            int indent = 0;
            var organisedSettings = SectionDataTableHelper.ConvertAnalysisResult(imageAnalysisResult, parsingProfile);
            foreach (var section in organisedSettings)
            {
                var columns = section.Settings.Columns.Cast<DataColumn>().ToList();
                sb.AppendLine($"<h2>{section.Name}</h2>");
                sb.AppendLine("<table>"); 
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
                                if (imagePathToAttachment.TryGetValue(screenshotPath, out var attachment))
                                {
                                    sb.AppendLine(
                                        $"{new string('\t', indent)}<ac:image ac:thumbnail=\"true\" ac:width=\"96\" ac:height=\"54\"><ri:attachment ri:filename=\"{attachment.Id}\"/></ac:image>\n");
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
                sb.AppendLine("</table>");
            }

            return sb.ToString();
        }
    }
}