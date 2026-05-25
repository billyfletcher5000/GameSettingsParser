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
        
        private readonly Dictionary<string, AuthenticationTokenModel> _sessionData = new();
        private readonly ILogService _log;
        
        public AppSettingsSessionStoreService(ILogService logService)
        {
            _log = logService;
            LoadSessionData();
        }
        
        public AuthenticationTokenModel? GetSessionToken(string clientId)
        {
            if (_sessionData.ContainsKey(clientId))
            {
                _log.Debug($"Found session data:\r\n\tClient ID: {clientId}\r\n\tToken:\r\n\t{_sessionData[clientId]}");
                return _sessionData[clientId];
            }

            _log.Debug($"No session data found for client ID: {clientId}, returning null");
            return null;
        }

        public void SetSessionToken(string clientId, AuthenticationTokenModel token)
        {
            _log.Debug($"Setting session data:\r\n\tClient ID: {clientId}\r\n\tToken:\r\n\t{token}");
            _sessionData[clientId] = token;
            SaveSessionData();
        }

        private void SaveSessionData()
        {
            _log.Debug("Saving session data to App Config");
            using (var writer = new StringWriter())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    Formatting = Formatting.Indented
                });
                serializer.Serialize(writer, _sessionData);
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                var serializedSessionData = writer.ToString();

                if (config.AppSettings.Settings[SessionDataKey] is null)
                {
                    config.AppSettings.Settings.Add(SessionDataKey, serializedSessionData);
                }
                else
                {
                    config.AppSettings.Settings[SessionDataKey].Value = serializedSessionData;
                }
                
                config.Save(ConfigurationSaveMode.Full);
                
                _log.Debug($"Session data saved to App Config:\r\n{writer.ToString()}");
            }
        }
        
        private void LoadSessionData()
        {
            _log.Debug("Loading session data from App Config");
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
                            _log.Log($"Duplicate key in session data, overwriting previous: {pair.Key}");
                        
                        _log.Debug($"Adding session data:\r\n\tClient ID: {pair.Key}\r\n\tToken:\r\n\t{pair.Value}");
                        _sessionData[pair.Key] = pair.Value;
                    }
                }
            }
        }
    }
}