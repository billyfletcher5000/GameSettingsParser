using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GameSettingsParser.Model.Authentication;
using GameSettingsParser.Services.Logging;
using GameSettingsParser.Services.SessionStore;
using GameSettingsParser.Services.UserState;

namespace GameSettingsParser.Services.Authentication
{
    public class OAuth2AuthenticationService : IAuthenticationService
    {
        private const int AuthenticationPort = 28282;
        private readonly IUserStateService _userStateService;
        private readonly ISessionStoreService _sessionStoreService;
        private readonly ILogService _log;
        
        public OAuth2AuthenticationService(IUserStateService userStateService, ISessionStoreService sessionStoreService, ILogService logService)
        {
            _userStateService = userStateService;
            _sessionStoreService = sessionStoreService;
            _log = logService;
        }
        
        public async Task<string?> AuthenticateAsync(AuthenticationOptions options)
        {
            _log.Debug($"Beginning authentication for options: {options}");
            
            var port = AuthenticationPort;
            var redirectUri = $"http://localhost:{port}/";
            
            var sessionToken = _sessionStoreService.GetSessionToken(options.ClientId);

            if (sessionToken.HasValue && sessionToken.Value.RefreshToken != null)
            {
                _log.Debug($"Existing token found, attempting to refresh: {sessionToken.Value}");
                var refreshedTokenModel = await RefreshTokenAsync(options.TokenEndpoint, options.ClientId, options.ClientSecret, redirectUri, sessionToken.Value.RefreshToken);
                if (refreshedTokenModel != null)
                {
                    _sessionStoreService.SetSessionToken(options.ClientId, refreshedTokenModel.Value);
                    return refreshedTokenModel.Value.AccessToken;
                }
            }
            
            var scope = options.Scope;
            scope += "%20offline_access";

            using var httpListener = new HttpListener();
            httpListener.Prefixes.Add(redirectUri);
            httpListener.Start();

            var state = _userStateService.GetUserState(options.ClientId);

            var authUrl = $"{options.AuthorizationEndpoint}?";

            if (options.Audience != null)
            {
                authUrl += $"audience={Uri.EscapeDataString(options.Audience)}&";
            }

            authUrl += $"client_id={Uri.EscapeDataString(options.ClientId)}" +
                       $"&scope={scope}" + 
                       $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                       $"&state={state}" +
                       $"&response_type=code" +
                       $"&prompt=consent";
            
            _log.Debug($"Performing full authorization with url: {authUrl}");

            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            var context = await httpListener.GetContextAsync();

            var query = context.Request.QueryString;
            var code = query["code"];
            var incomingState = query["state"];

            await using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                await writer.WriteAsync("<html><body><h2>Authentication successful! You can close this tab.</h2></body></html>");
            }
            
            context.Response.Close();
            httpListener.Stop();
            
            _log.Debug("Authorization successful!");
            
            if (incomingState != null && incomingState != state)
            {
                throw new InvalidOperationException("State verification failed. Possible Cross-Site Request Forgery attack.");
            }

            if (string.IsNullOrEmpty(code))
            {
                _log.Warning("Authorization code was not returned by the server.");
                return null;
            }

            //TODO: Add support for different versions of this function for non-json content-type and challenge-code based alternatives
            var tokenModel = await ExchangeCodeForTokenJsonAsync(options.TokenEndpoint, options.ClientId, options.ClientSecret, code, redirectUri);
            if (tokenModel != null)
            {
                _sessionStoreService.SetSessionToken(options.ClientId, tokenModel.Value);
                return tokenModel.Value.AccessToken;
            }

            return null;
        }

        private async Task<AuthenticationTokenModel?> ExchangeCodeForTokenJsonAsync(string tokenEndpoint, string clientId, string clientSecret, string code, string redirectUri)
        {
            _log.Debug($"Attempting to exchange code for token: code: {code}, redirectUri: {redirectUri}, clientId: {clientId}, clientSecret: {clientSecret}, tokenEndpoint: {tokenEndpoint}");
            
            using var httpClient = new HttpClient();
            var requestBody = new AuthenticationTokenExchangeRequestModel
            {
                GrantType = "authorization_code",
                ClientId = clientId,
                ClientSecret = clientSecret,
                Code = code,
                RedirectUri = redirectUri,
            };

            using var content = JsonContent.Create(requestBody);
            var response = await httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Token exchange failed: {responseContent}");
            }
            
            await using var responseStream = await response.Content.ReadAsStreamAsync();

            var tokenResponse = await JsonSerializer.DeserializeAsync<AuthenticationTokenModel>(
                responseStream,
                JsonOptions.Default);
            
            _log.Debug($"Token exchange successful: {tokenResponse}");
            
            return tokenResponse;
        }
        
        
        private async Task<AuthenticationTokenModel?> RefreshTokenAsync(string tokenEndpoint, string clientId, string clientSecret, string redirectUri, string refreshToken)
        {
            _log.Debug($"Attempting to refresh token: refreshToken: {refreshToken}, redirectUri: {redirectUri}, clientId: {clientId}, clientSecret: {clientSecret}, tokenEndpoint: {tokenEndpoint}");
            
            using var httpClient = new HttpClient();
            var requestBody = new AuthenticationTokenExchangeRequestModel
            {
                GrantType = "refresh_token",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUri,
                RefreshToken = refreshToken
            };

            using var content = JsonContent.Create(requestBody);
            var response = await httpClient.PostAsync(tokenEndpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _log.Warning($"Token exchange failed: {responseContent}");
                return null;
            }
            
            await using var responseStream = await response.Content.ReadAsStreamAsync();

            var tokenResponse = await JsonSerializer.DeserializeAsync<AuthenticationTokenModel>(
                responseStream,
                JsonOptions.Default);
            
            _log.Debug($"Token refresh successful: {tokenResponse}");
            
            return tokenResponse;
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