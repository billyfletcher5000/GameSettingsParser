using System.Data;
using System.Diagnostics;
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
        private const string Scope = "read:content-details:confluence%20read:page:confluence%20write:page:confluence%20write:attachment:confluence%20read:space:confluence";
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
            var dialogViewModel = new ConfluenceExportDialogViewModel(_confluenceApiService, accessToken);
            var dialog = new ConfluenceExportDialog(dialogViewModel);

            if (dialog.ShowDialog() != true || dialogViewModel.Config.Page == null)
                return;

            var exportConfig = dialogViewModel.Config;
            var cancellationToken = CancellationToken.None;
            
            
            // Upload image attachments
            foreach (var processedImage in imageAnalysisResult.ProcessedImages)
            {
                var imagePath = processedImage.ScreenshotPath;
                var contentType = ImageContentTypeHelper.GetImageContentType(imagePath);
                
                await using var attachmentStream = File.OpenRead(imagePath);
                
                await _confluenceApiService.UploadAttachmentToPageAsync(
                    accessToken,
                    exportConfig.SiteId,
                    exportConfig.Page.Id!,
                    attachmentStream,
                    Path.GetFileName(processedImage.ScreenshotPath),
                    contentType,
                    cancellationToken);
            }
            
            var content = GenerateConfluenceStorageFormatTableFromAnalysisResult(imageAnalysisResult, parsingProfile);
            
            var pageContent = exportConfig.Page.Body is { Storage: not null } ? exportConfig.Page.Body.Storage.Value : string.Empty;

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
            
            var url = await _confluenceApiService.UpdatePageBodyAsync(accessToken, exportConfig.SiteId, exportConfig.Page.Id!, exportConfig.Page.Title!, exportConfig.Page.Version!.Number, pageContent, cancellationToken);

            if (url != null)
            {
                Console.WriteLine($"Successfully updated page: {url}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }

        private static string GenerateConfluenceStorageFormatTableFromAnalysisResult(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            StringBuilder sb = new();

            var organisedSettings = SectionDataTableHelper.ConvertAnalysisResult(imageAnalysisResult, parsingProfile);
            foreach (var section in organisedSettings)
            {
                var columns = section.Settings.Columns.Cast<DataColumn>().ToList();
                sb.Append($"<h2>{section.Name}</h2>");
                sb.Append("<table>");
                sb.Append($"<tr>"); 
                foreach (var column in columns)
                {
                    var markupType = parsingProfile.MarkupTypes.FirstOrDefault(item => item.Name.Equals(column.ColumnName));
                    var columnWidth = markupType != null ? markupType.HTMLExportTableWidth : string.Empty;
                    if(!string.IsNullOrEmpty(columnWidth))
                        sb.Append($"<th width=\"{columnWidth}\">{column.ColumnName}</th>");
                    else
                        sb.Append($"<th>{column.ColumnName}</th>");
                }
                sb.Append($"</tr>");
                
                foreach (var row in section.Settings.Rows.Cast<DataRow>())
                {
                    sb.Append($"<tr>");
                    foreach (var column in columns)
                    {
                        string? cellValue = row[column] as string;
                        if (cellValue == null)
                            continue;
                        
                        sb.Append($"<td>");
                        if (column.ColumnName == SectionDataTableHelper.ScreenshotColumnName)
                        {
                            var screenshotPaths = cellValue.Split(SectionDataTableHelper.ListSeparator);
                            foreach (var screenshotPath in screenshotPaths)
                            {
                                sb.Append($"<ac:image ac:thumbnail=\"true\" ac:width=\"96\" ac:height=\"54\"><ri:attachment ri:filename=\"{Path.GetFileName(screenshotPath)}\"/></ac:image>\n");
                            }
                        }
                        else
                        {
                            sb.Append($"{cellValue}");
                        }
                        sb.Append($"</td>");
                    }
                    sb.Append($"</tr>");
                }
                sb.Append("</table>");
            }

            return sb.ToString();
        }
    }
}