using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using GameSettingsParser.Model;
using GameSettingsParser.Model.Atlassian;
using GameSettingsParser.Services.Authentication;
using GameSettingsParser.Services.Confluence;
using GameSettingsParser.Services.KeyVault;
using GameSettingsParser.Services.Logging;
using GameSettingsParser.Services.Windows;
using GameSettingsParser.Utility;
using GameSettingsParser.ViewModels;
using GameSettingsParser.Views.AnalysisExport;

namespace GameSettingsParser.Services.AnalysisExport
{
    public class ConfluenceAnalysisExportService : IAnalysisExportService
    {
        private const string Audience = "api.atlassian.com";
        private const string ClientIdVaultName = "Confluence_Client_Id";
        private const string ClientSecretVaultName = "Confluence_Client_Secret";
        private const string Scope = "read:content-details:confluence%20read:page:confluence%20write:page:confluence%20write:attachment:confluence%20read:space:confluence";
        private const string AuthorizationEndpoint = "https://auth.atlassian.com/authorize";
        private const string TokenEndpoint = "https://auth.atlassian.com/oauth/token";
        
        public bool SupportsExportToWebsite => true;

        private readonly IAuthenticationService _authenticationService;
        private readonly IKeyVaultService _keyVaultService;
        private readonly ConfluenceApiService _confluenceApiService;
        private readonly ILogService _log;
        private readonly IWindowService _windowService;
        
        public ConfluenceAnalysisExportService(IAuthenticationService authenticationService, IKeyVaultService keyVaultService, ConfluenceApiService confluenceApiService, ILogService logService, IWindowService windowService)
        {
           _authenticationService = authenticationService;
           _keyVaultService = keyVaultService;
           _confluenceApiService = confluenceApiService;
           _log = logService;
           _windowService = windowService;
        }
        
        public async Task ExportToWebsiteAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile, CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage)
        {
            var authenticationOptions = new AuthenticationOptions()
            {
                Audience = Audience,
                AuthorizationEndpoint = AuthorizationEndpoint,
                ClientId = _keyVaultService.GetClientSecret(ClientIdVaultName)!,
                ClientSecret = _keyVaultService.GetClientSecret(ClientSecretVaultName)!,
                Scope = Scope,
                TokenEndpoint = TokenEndpoint,
            };

            progressText.Report("Confluence Export: Authenticating...");
            progressPercentage.Report(0.0);
            
            var accessToken = await Task.Run(() => _authenticationService.AuthenticateAsync(authenticationOptions), cancellationToken);

            if (accessToken == null)
                return;
            
            progressText.Report("Confluence Export: Authentication complete");
            progressPercentage.Report(1.0);
            
            // Should this be done in a service? It seems anti-pattern as it's view related but also a very useful way to do things
            var dialogViewModel = new ConfluenceExportDialogViewModel(_confluenceApiService, accessToken);
            var dialog = new ConfluenceExportDialog(dialogViewModel);

            if (_windowService.ShowDialog(dialog) != true || dialogViewModel.Config.Page == null)
                return;
            
            await Task.Run(() => UpdatePageAsync(imageAnalysisResult, parsingProfile, cancellationToken, progressText, progressPercentage, dialogViewModel, accessToken), cancellationToken);
        }

        private async Task UpdatePageAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile,
            CancellationToken cancellationToken, IProgress<string> progressText, IProgress<double> progressPercentage,
            ConfluenceExportDialogViewModel dialogViewModel, string accessToken)
        {
            progressText.Report("Confluence Export: Uploading image attachments...");
            progressPercentage.Report(0.0);

            var exportConfig = dialogViewModel.Config;
            
            var numImages = imageAnalysisResult.ProcessedImages.Count;
            var imageIndex = 0;
            
            // Upload image attachments
            foreach (var processedImage in imageAnalysisResult.ProcessedImages)
            {
                progressText.Report($"Confluence Export: Uploading image attachment {imageIndex + 1} of {numImages}: {Path.GetFileName(processedImage.ScreenshotPath)}");
                progressPercentage.Report((double)imageIndex / numImages);
                
                var imagePath = processedImage.ScreenshotPath;
                var contentType = ImageContentTypeHelper.GetImageContentType(imagePath);
                
                await using var attachmentStream = File.OpenRead(imagePath);
                
                await _confluenceApiService.UploadAttachmentToPageAsync(
                    accessToken,
                    exportConfig.SiteId,
                    exportConfig.Page!.Id!,
                    attachmentStream,
                    Path.GetFileName(processedImage.ScreenshotPath),
                    contentType,
                    cancellationToken);
                
                imageIndex++;
            }
            
            progressText.Report("Confluence Export: Uploading image attachments complete");
            progressPercentage.Report(1.0);
            
            var content = GenerateConfluenceStorageFormatTableFromAnalysisResult(imageAnalysisResult, parsingProfile);
            
            var pageContent = exportConfig.Page!.Body is { Storage: not null } ? exportConfig.Page.Body.Storage.Value : string.Empty;

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
            
            progressText.Report("Confluence Export: Updating page...");
            progressPercentage.Report(0.0);
            var url = await _confluenceApiService.UpdatePageBodyAsync(accessToken, exportConfig.SiteId, exportConfig.Page.Id!, exportConfig.Page.Title!, exportConfig.Page.Version!.Number, pageContent, cancellationToken);

            if (url != null)
            {
                progressText.Report("Confluence Export: Page update complete");
                _log.Log($"Successfully updated page: {url}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else
            {
                progressText.Report("Confluence Export: Page update failed");
                _log.Warning("Failed to update page");
            }
            
            progressPercentage.Report(1.0);
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