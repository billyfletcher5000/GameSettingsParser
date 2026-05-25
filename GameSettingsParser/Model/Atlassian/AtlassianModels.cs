using System.Text.Json.Serialization;

namespace GameSettingsParser.Model.Atlassian
{
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
        [JsonPropertyName("_links")]
        public ConfluenceLinks? Links { get; set; }
    }

    public sealed class ConfluenceLinks
    {
        [JsonPropertyName("next")]
        public string? Next { get; set; }
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
        
        [JsonPropertyName("_links")]
        public ConfluenceLinks? Links { get; set; }
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
        
        [JsonPropertyName("body")]
        public ConfluencePageBodyBulk? Body { get; init; }
    }

    public sealed class ConfluencePageBodyBulk
    {
        [JsonPropertyName("storage")]
        public ConfluenceBodyType? Storage { get; set; }
        
        [JsonPropertyName("atlas_doc_format")]
        public ConfluenceBodyType? AtlasDocFormat { get; set; }
    }

    public sealed class ConfluenceBodyType
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("representation")]
        public string? Representation { get; set; }
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
}