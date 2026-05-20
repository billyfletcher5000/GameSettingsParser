using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameSettingsParser.Services.Confluence
{
    public sealed class ConfluenceApiService
    {
        private readonly HttpClient _httpClient;

        public ConfluenceApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}