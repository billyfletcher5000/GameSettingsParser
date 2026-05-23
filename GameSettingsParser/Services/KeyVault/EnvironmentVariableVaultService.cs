namespace GameSettingsParser.Services.KeyVault
{
    public class EnvironmentVariableVaultService : IKeyVaultService
    {
        private static readonly Dictionary<string, string> SecretNameToEnvVariable = new()
        {
            { "Confluence_Client_Id", "CONFLUENCE_CLIENT_ID" },
            { "Confluence_Client_Secret", "CONFLUENCE_CLIENT_SECRET" }
        };
        
        public string? GetClientSecret(string secretName)
        {
            if (SecretNameToEnvVariable.TryGetValue(secretName, out var envVar) && envVar != null)
            {
                return Environment.GetEnvironmentVariable(envVar)
                       ?? throw new InvalidOperationException(
                           $"{envVar} environment variable not set");
            }

            return null;
        }
    }
}