using GameSettingsParser.Model;
using GameSettingsParser.Services.Authentication;
using GameSettingsParser.Services.KeyVault;
using GameSettingsParser.ViewModels;
using GameSettingsParser.Views;

namespace GameSettingsParser.Services.AnalysisExport
{
    public class ConfluenceAnalysisExportService : IAnalysisExportService
    {
        private const string Audience = "";
        private const string ClientId = "";
        private const string ClientSecretKeyId = "";
        private const string Scope = "read:page:confluence write:page:confluence read:space:confluence";
        private const string AuthorizationEndpoint = "";
        private const string TokenEndpoint = "";
        
        public bool SupportsExportToWebsite => true;

        private readonly IAuthenticationService _authenticationService;
        private readonly IKeyVaultService _keyVaultService;
        
        public ConfluenceAnalysisExportService(IAuthenticationService authenticationService, IKeyVaultService keyVaultService)
        {
           _authenticationService = authenticationService;
           _keyVaultService = keyVaultService;
        }
        
        public async Task ExportToWebsiteAsync(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            var authenticationOptions = new AuthenticationOptions()
            {
                Audience = Audience,
                AuthorizationEndpoint = AuthorizationEndpoint,
                ClientId = ClientId,
                ClientSecret = _keyVaultService.GetClientSecret(ClientSecretKeyId),
                Scope = Scope,
                TokenEndpoint = TokenEndpoint,
            };

            var accessToken = await _authenticationService.AuthenticateAsync(authenticationOptions);
            
            
            // Should this be done in a service? It seems anti-pattern as it's view related but also a very useful way to do things
            ConfluenceExportDialogViewModel dialogViewModel = new();
            ConfluenceExportDialog dialog = new ConfluenceExportDialog(dialogViewModel);

            if (dialog.ShowDialog() == true)
            {
            }
        }
    }
}