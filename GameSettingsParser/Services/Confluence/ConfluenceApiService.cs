using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GameSettingsParser.Model.Atlassian;
using GameSettingsParser.Services.Logging;

namespace GameSettingsParser.Services.Confluence
{
    public sealed class ConfluenceApiService
    {
        private readonly HttpClient _httpClient = new();

        private ILogService _log;
        
        public ConfluenceApiService(ILogService log)
        {
            _log = log;
        }
        
        public async Task<IReadOnlyList<AtlassianAccessibleResource>> GetConfluenceAccessibleResourcesAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            var allAccessibleResources = await GetAccessibleResourcesAsync(accessToken, cancellationToken);
            return allAccessibleResources.Where(r => r.Scopes.Any(s => s.Contains("confluence", StringComparison.OrdinalIgnoreCase))).ToList();
        }

        private async Task<IReadOnlyList<AtlassianAccessibleResource>> GetAccessibleResourcesAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.atlassian.com/oauth/token/accessible-resources");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            return await JsonSerializer.DeserializeAsync<List<AtlassianAccessibleResource>>(
                       stream,
                       JsonOptions.Default,
                       cancellationToken)
                   ?? [];
        }

        public async Task<IReadOnlyList<ConfluenceSpace>> GetSpacesAsync(
            string accessToken,
            string cloudId,
            CancellationToken cancellationToken)
        {
            var spaces = new List<ConfluenceSpace>();

            var baseUri = new Uri(
                $"https://api.atlassian.com/ex/confluence/{Uri.EscapeDataString(cloudId)}/");

            Uri? requestUri = new Uri(baseUri, "wiki/api/v2/spaces?limit=250");

            while (requestUri is not null)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var page = await response.Content.ReadFromJsonAsync<ConfluenceSpaceResponse>(
                    cancellationToken);

                if (page?.Results is { Count: > 0 })
                {
                    spaces.AddRange(page.Results);
                }

                requestUri = ResolveConfluenceNextUri(baseUri, page?.Links?.Next);
            }

            return spaces;

        }
        
        private static Uri? ResolveConfluenceNextUri(Uri baseUri, string? nextLink)
        {
            if (string.IsNullOrWhiteSpace(nextLink))
            {
                return null;
            }

            if (Uri.TryCreate(nextLink, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri;
            }

            var relativeLink = nextLink.TrimStart('/');

            return new Uri(baseUri, relativeLink);
        }

        
        public async Task<IReadOnlyList<ConfluencePage>> GetPagesFromSpaceAsync(
            string accessToken,
            string cloudId,
            string spaceId,
            CancellationToken cancellationToken)
        {
            var baseUri = new Uri(
                $"https://api.atlassian.com/ex/confluence/{Uri.EscapeDataString(cloudId)}/");

            if (string.IsNullOrWhiteSpace(spaceId))
            {
                return [];
            }

            var pages = new List<ConfluencePage>();

            Uri? requestUri = new Uri(
                baseUri,
                $"wiki/api/v2/spaces/{Uri.EscapeDataString(spaceId)}/pages?limit=250&body-format=storage&sort=title");

            while (requestUri is not null)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var page = await response.Content.ReadFromJsonAsync<ConfluencePageResponse>(
                    JsonOptions.Default,
                    cancellationToken);

                if (page?.Results is { Count: > 0 })
                {
                    pages.AddRange(page.Results);
                }

                requestUri = ResolveConfluenceNextUri(baseUri, page?.Links?.Next);
            }

            return pages;

        }
        
        public async Task<ConfluenceAttachment?> UploadAttachmentToPageAsync(
            string accessToken,
            string cloudId,
            string pageId,
            Stream attachmentStream,
            string fileName,
            string? contentType,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://api.atlassian.com/ex/confluence/{cloudId}/wiki/rest/api/content/{pageId}/child/attachment");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("X-Atlassian-Token", "no-check");

            using var formData = new MultipartFormDataContent();

            if (attachmentStream.CanSeek)
                attachmentStream.Position = 0;
            
            var fileContent = new StreamContent(attachmentStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            formData.Add(fileContent, "file", fileName);
            formData.Add(new StringContent("true"), "minorEdit");
            
            request.Content = formData;

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _log.Error($"Exception occurred when attempting to upload \"{fileName}\":\n\tResponseBody: {responseBody}\n\tException: {e}");
            }
            

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var attachmentResponse = await JsonSerializer.DeserializeAsync<ConfluenceAttachmentResponse>(
                responseStream,
                JsonOptions.Default,
                cancellationToken);

            return attachmentResponse?.Results.FirstOrDefault();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="cloudId"></param>
        /// <param name="pageId"></param>
        /// <param name="title"></param>
        /// <param name="currentVersionNumber"></param>
        /// <param name="pageBodyInStorageFormat"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>URL to the updated page if successful, null if not</returns>
        public async Task<string?> UpdatePageBodyAsync(
            string accessToken,
            string cloudId,
            string pageId,
            string title,
            int currentVersionNumber,
            string pageBodyInStorageFormat,
            CancellationToken cancellationToken = default)
        {
            var baseUri = new Uri(
                $"https://api.atlassian.com/ex/confluence/{Uri.EscapeDataString(cloudId)}/");

            var requestUri = new Uri(
                baseUri,
                $"wiki/api/v2/pages/{Uri.EscapeDataString(pageId)}");
            
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUri);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                id = pageId,
                status = "current",
                title,
                version = new
                {
                    number = currentVersionNumber + 1,
                    message = $"Updated page body from Game Settings Parser"
                },
                body = new
                {
                    storage = new
                    {
                        value = pageBodyInStorageFormat,
                        representation = "storage"
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var jsonDoc = JsonDocument.Parse(responseContent);
            
            var links = jsonDoc.RootElement.GetProperty("_links");
            if (links.TryGetProperty("webui", out var webUiElement) && links.TryGetProperty("base", out var baseElement))
            {
                var baseString = baseElement.GetString();
                var webUiString = webUiElement.GetString();
                if (string.IsNullOrWhiteSpace(baseString) || string.IsNullOrWhiteSpace(webUiString))
                    return null;
                
                var pageUrl = baseString + webUiString;
                return pageUrl;
            }
            
            return null;
        }
        
        internal static class JsonOptions
        {
            public static readonly JsonSerializerOptions Default = new()
            {
                PropertyNameCaseInsensitive = true
            };
        }
    }
}