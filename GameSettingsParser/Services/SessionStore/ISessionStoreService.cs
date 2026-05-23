using GameSettingsParser.Model.Authentication;

namespace GameSettingsParser.Services.SessionStore
{
    public interface ISessionStoreService
    {
        public AuthenticationTokenModel? GetSessionToken(string clientId);
        public void SetSessionToken(string clientId, AuthenticationTokenModel token);
    }
}