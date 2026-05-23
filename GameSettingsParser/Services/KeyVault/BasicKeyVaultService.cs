namespace GameSettingsParser.Services.KeyVault
{
    public class BasicKeyVaultService : IKeyVaultService
    {
        private static readonly Dictionary<string, string> SecretNameToSecret = new()
        {
            { "Atlassian", "" }
        };
        
        public string? GetClientSecret(string secretName)
        {
            return SecretNameToSecret[secretName];
        }
    }
}