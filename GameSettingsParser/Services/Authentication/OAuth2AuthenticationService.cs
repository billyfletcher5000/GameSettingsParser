using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GameSettingsParser.Services.UserState;

namespace GameSettingsParser.Services.Authentication
{
    public class OAuth2AuthenticationService : IAuthenticationService
    {
        private readonly IUserStateService _userStateService;
        
        public OAuth2AuthenticationService(IUserStateService userStateService)
        {
            _userStateService = userStateService;
        }
        
        public async Task<string> AuthenticateAsync(AuthenticationOptions options)
        {
            var port = GetRandomUnusedPort();
            var redirectUri = $"http://localhost:{port}/";

            using var httpListener = new HttpListener();
            httpListener.Prefixes.Add(redirectUri);
            httpListener.Start();
            
            var state = _userStateService.GetUserState();

            var authUrl = $"{options.AuthorizationEndpoint}?response_type=code" +
                               $"&client_id={Uri.EscapeDataString(options.ClientId)}" +
                               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                               $"&state={state}" +
                               $"&response_type=code" +
                               $"&prompt=consent";

            if (options.Audience != null)
            {
                authUrl += $"&audience={Uri.EscapeDataString(options.Audience)}";
            }
            
            if (options.Scope != null)
            {
                authUrl += $"&scope={Uri.EscapeDataString(options.Scope)}";
            }

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
            
            if (incomingState != null && incomingState != state)
            {
                throw new InvalidOperationException("State verification failed. Possible Cross-Site Request Forgery attack.");
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new Exception("Authorization code was not returned by the server.");
            }

            //TODO: Add support for different versions of this function for non-json content-type and challenge-code based alternatives
            return await ExchangeCodeForTokenJsonAsync(options.TokenEndpoint, options.ClientId, options.ClientSecret, code, redirectUri);
        }

        private static async Task<string> ExchangeCodeForTokenJsonAsync(string tokenEndpoint, string clientId, string clientSecret, string code, string redirectUri)
        {
            using var httpClient = new HttpClient();
            var requestBody = new
            {
                grant_type = "authorization_code",
                client_id = clientId,
                client_secret = clientSecret,
                code,
                redirect_uri = redirectUri
            };

            using var content = JsonContent.Create(requestBody);
            var response = await httpClient.PostAsync(tokenEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token exchange failed: {responseContent}");
            }

            using var jsonDoc = JsonDocument.Parse(responseContent);
            var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
            return accessToken ?? throw new Exception("Access token not found in the response.");
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}