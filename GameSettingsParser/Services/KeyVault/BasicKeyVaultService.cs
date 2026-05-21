namespace GameSettingsParser.Services.KeyVault
{
    public class BasicKeyVaultService : IKeyVaultService
    {
        private static readonly Dictionary<string, string> SecretNameToSecret = new()
        {
        };
        
        public string? GetClientSecret(string secretName)
        {
            return SecretNameToSecret[secretName];
        }
    }
}