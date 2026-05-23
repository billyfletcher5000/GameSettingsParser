using System.Configuration;
using System.IO;
using GameSettingsParser.Model.Authentication;
using GameSettingsParser.Services.Logging;
using GameSettingsParser.Settings;
using Newtonsoft.Json;

namespace GameSettingsParser.Services.SessionStore
{
    public class AppSettingsSessionStoreService : ISessionStoreService
    {
        public const string SessionDataKey = "SessionData";
        
        private Dictionary<string, AuthenticationTokenModel> _sessionData = new();

        private ILogService _log;
        
        public AppSettingsSessionStoreService(ILogService logService)
        {
            _log = logService;
            LoadSessionData();
        }
        
        public AuthenticationTokenModel? GetSessionToken(string clientId)
        {
            if(_sessionData.ContainsKey(clientId))
                return _sessionData[clientId];

            return null;
        }

        public void SetSessionToken(string clientId, AuthenticationTokenModel token)
        {
            _sessionData[clientId] = token;
            SaveSessionData();
        }

        private void SaveSessionData()
        {
            using (var writer = new StringWriter())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    Formatting = Formatting.Indented
                });
                serializer.Serialize(writer, _sessionData);
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Add(SessionDataKey, writer.ToString());
                config.Save(ConfigurationSaveMode.Modified);
            }
        }
        
        private void LoadSessionData()
        {
            _sessionData.Clear();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
            var value = config.AppSettings.Settings[SessionDataKey]?.Value;
            if (value == null)
                return;
            
            using (var reader = new StringReader(value))
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });

                if (serializer.Deserialize(
                        reader,
                        typeof(Dictionary<string, AuthenticationTokenModel>)) is Dictionary<string, AuthenticationTokenModel> sessionDataSet)
                {
                    foreach (var pair in sessionDataSet)
                    {
                        if(_sessionData.ContainsKey(pair.Key)) 
                            System.Diagnostics.Debug.WriteLine($"Duplicate key in session data, overwriting previous: {pair.Key}");
                        
                        _sessionData[pair.Key] = pair.Value;
                    }
                }
            }
        }
    }
}