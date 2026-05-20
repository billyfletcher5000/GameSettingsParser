namespace GameSettingsParser.Services.Authentication
{
    public class AuthenticationOptions
    {
        public required string ClientId { get; init; } = string.Empty;
        public required string ClientSecret { get; init; } = string.Empty;
        public required string AuthorizationEndpoint { get; init; } = string.Empty;
        public required string TokenEndpoint { get; init; } = string.Empty;
        public string? Audience { get; set; } = null;
        public string? Scope { get; set; } = null;
    }
    
    public interface IAuthenticationService
    {
        public Task<string> AuthenticateAsync(AuthenticationOptions options);
    }
}