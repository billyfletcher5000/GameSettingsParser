using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameSettingsParser.Services.Confluence
{
    public sealed class ConfluenceApiService
    {
        private readonly HttpClient _httpClient = new();

        public ConfluenceApiService()
        {
        }

        public async Task<IReadOnlyList<AtlassianAccessibleResource>> GetConfluenceAccessibleResourcesAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            var allAccessibleResources = await GetAccessibleResourcesAsync(accessToken, cancellationToken);
            return allAccessibleResources.Where(r => r.Scopes.Contains("confluence", StringComparer.OrdinalIgnoreCase)).ToList();
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

            const int limit = 50;
            var start = 0;

            while (true)
            {
                var url =
                    $"https://api.atlassian.com/ex/confluence/{Uri.EscapeDataString(cloudId)}/wiki/rest/api/space" +
                    $"?limit={limit}&start={start}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var page = await JsonSerializer.DeserializeAsync<ConfluenceSpaceResponse>(
                    stream,
                    JsonOptions.Default,
                    cancellationToken);

                if (page?.Results is null || page.Results.Count == 0)
                {
                    break;
                }

                spaces.AddRange(page.Results);

                if (page.Results.Count < limit)
                {
                    break;
                }

                start += limit;
            }

            return spaces;
        }
        
        public async Task<IReadOnlyList<ConfluencePage>> GetPagesFromSpaceAsync(
            string accessToken,
            string cloudId,
            string spaceKey,
            CancellationToken cancellationToken)
        {
            var pages = new List<ConfluencePage>();

            const int limit = 50;
            var start = 0;

            while (true)
            {
                var url =
                    $"https://api.atlassian.com/ex/confluence/{Uri.EscapeDataString(cloudId)}/wiki/rest/api/content" +
                    $"?type=page" +
                    $"&spaceKey={Uri.EscapeDataString(spaceKey)}" +
                    $"&limit={limit}" +
                    $"&start={start}" +
                    $"&expand=version,history,_links";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var page = await JsonSerializer.DeserializeAsync<ConfluencePageResponse>(
                    stream,
                    JsonOptions.Default,
                    cancellationToken);

                if (page?.Results is null || page.Results.Count == 0)
                {
                    break;
                }

                pages.AddRange(page.Results);

                if (page.Results.Count < limit)
                {
                    break;
                }

                start += limit;
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
                HttpMethod.Post,
                $"https://api.atlassian.com/ex/confluence/{cloudId}/wiki/rest/api/content/{pageId}/child/attachment");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("X-Atlassian-Token", "no-check");

            using var formData = new MultipartFormDataContent();

            var fileContent = new StreamContent(attachmentStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            formData.Add(fileContent, "file", fileName);

            request.Content = formData;

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var attachmentResponse = await JsonSerializer.DeserializeAsync<ConfluenceAttachmentResponse>(
                responseStream,
                JsonOptions.Default,
                cancellationToken);

            return attachmentResponse?.Results.FirstOrDefault();
        }
        
        public async Task<string> GetPageStorageBodyAsync(
            string accessToken,
            string cloudId,
            string pageId,
            CancellationToken cancellationToken = default)
        {
            var requestUrl =
                $"https://api.atlassian.com/ex/confluence/{cloudId}/wiki/rest/api/content/{pageId}?expand=body.storage";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var page = await response.Content.ReadFromJsonAsync<ConfluencePageStorageResponse>(
                cancellationToken: cancellationToken);
            
            return page?.Body?.Storage?.Value
                   ?? throw new InvalidOperationException("Confluence response did not contain body.storage.value.");
        }
        
        public async Task UpdatePageStorageBodyAsync(
            string accessToken,
            string cloudId,
            string pageId,
            string title,
            int currentVersionNumber,
            string pageBodyInStorageFormat,
            CancellationToken cancellationToken = default)
        {
            var url =
                $"https://api.atlassian.com/ex/confluence/{cloudId}/wiki/rest/api/content/{pageId}";

            using var request = new HttpRequestMessage(HttpMethod.Put, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                id = pageId,
                type = "page",
                title,
                version = new
                {
                    number = currentVersionNumber + 1
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
        }

    }

    public sealed class AtlassianAccessibleResource
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("url")]
        public required string Url { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; init; } = [];
    }

    public sealed class ConfluenceSpaceResponse
    {
        [JsonPropertyName("results")]
        public List<ConfluenceSpace> Results { get; init; } = [];
    }

    public sealed class ConfluenceSpace
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("key")]
        public string? Key { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }

    public sealed class ConfluencePageResponse
    {
        [JsonPropertyName("results")]
        public List<ConfluencePage> Results { get; init; } = [];
    }

    public sealed class ConfluencePage
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("_links")]
        public ConfluenceLinks? Links { get; init; }

        [JsonPropertyName("version")]
        public ConfluenceVersion? Version { get; init; }

        [JsonPropertyName("history")]
        public ConfluenceHistory? History { get; init; }
    }
    
    public sealed class ConfluencePageStorageResponse
    {
        [JsonPropertyName("body")]
        public ConfluencePageBody? Body { get; set; }
    }

    public sealed class ConfluencePageBody
    {
        [JsonPropertyName("storage")]
        public ConfluenceStorageBody? Storage { get; set; }
    }

    public sealed class ConfluenceStorageBody
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("representation")]
        public string? Representation { get; set; }
    }

    public sealed class ConfluenceLinks
    {
        [JsonPropertyName("webui")]
        public string? WebUi { get; init; }

        [JsonPropertyName("self")]
        public string? Self { get; init; }
    }

    public sealed class ConfluenceVersion
    {
        [JsonPropertyName("number")]
        public int Number { get; init; }
    }

    public sealed class ConfluenceHistory
    {
        [JsonPropertyName("createdDate")]
        public DateTimeOffset? CreatedDate { get; init; }

        [JsonPropertyName("lastUpdated")]
        public ConfluenceLastUpdated? LastUpdated { get; init; }
    }

    public sealed class ConfluenceLastUpdated
    {
        [JsonPropertyName("when")]
        public DateTimeOffset? When { get; init; }
    }
    
    public class ConfluenceAttachmentResponse
    {
        [JsonPropertyName("results")]
        public List<ConfluenceAttachment> Results { get; set; } = [];
    }

    public class ConfluenceAttachment
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("metadata")]
        public ConfluenceAttachmentMetadata? Metadata { get; set; }

        [JsonPropertyName("extensions")]
        public ConfluenceAttachmentExtensions? Extensions { get; set; }

        [JsonPropertyName("_links")]
        public ConfluenceLinks? Links { get; set; }

        [JsonPropertyName("version")]
        public ConfluenceVersion? Version { get; set; }
    }

    public class ConfluenceAttachmentMetadata
    {
        [JsonPropertyName("mediaType")]
        public string? MediaType { get; set; }
    }

    public class ConfluenceAttachmentExtensions
    {
        [JsonPropertyName("mediaType")]
        public string? MediaType { get; set; }

        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }

    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}