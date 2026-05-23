using System.Text.Json.Serialization;

namespace GameSettingsParser.Model.Authentication
{
    public struct AuthenticationTokenModel
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
        
        [JsonPropertyName("expires_at")]
        public int? ExpiresAt { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonIgnore]
        public DateTime? ExpirationTime
        {
            get
            {
                if (ExpiresIn is > 0)
                    return DateTime.UtcNow.AddSeconds(ExpiresIn.Value);
                
                return null;
            }
        }

        [JsonIgnore]
        public bool IsExpired => ExpirationTime.HasValue && DateTime.UtcNow >= ExpirationTime.Value;

        [JsonIgnore]
        public bool IsValid => !IsExpired;

        public AuthenticationTokenModel()
        {
        }
    }
    
    public class AuthenticationTokenExchangeRequestModel
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; } = "authorization_code";
    
        [JsonPropertyName("client_id")]
        public required string ClientId { get; set; }
    
        [JsonPropertyName("client_secret")]
        public required string ClientSecret { get; set; }
    
        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }
    
        [JsonPropertyName("redirect_uri")]
        public required string RedirectUri { get; set; }
    
        [JsonPropertyName("refresh_token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; set; }
    }
}