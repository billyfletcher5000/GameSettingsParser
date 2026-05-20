namespace GameSettingsParser.Services.UserState
{
    public class BasicUserStateService : IUserStateService
    {
        private string? _userState = null;
        
        public string GetUserState()
        {
            return _userState ??= Guid.NewGuid().ToString("N");
        }
    }
}