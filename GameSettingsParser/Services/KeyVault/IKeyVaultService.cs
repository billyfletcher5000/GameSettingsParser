namespace GameSettingsParser.Services.KeyVault
{
    public interface IKeyVaultService
    {
        public string GetClientSecret(string secretName);
    }
}