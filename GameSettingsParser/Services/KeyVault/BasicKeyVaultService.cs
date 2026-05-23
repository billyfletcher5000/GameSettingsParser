namespace GameSettingsParser.Services.KeyVault
{
    public class BasicKeyVaultService : IKeyVaultService
    {
        private static readonly Dictionary<string, string> SecretNameToSecret = new()
        {
            { "Atlassian", "ATOAM8nELmRdPRR-AACmkzxalwSx9Lw6851DeusPcaGT-b_cIMAyU2F_Fd0jt70tkZjy0F8EF994" }
        };
        
        public string? GetClientSecret(string secretName)
        {
            return SecretNameToSecret[secretName];
        }
    }
}