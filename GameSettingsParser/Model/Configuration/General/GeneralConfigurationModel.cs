using GameSettingsParser.Constants;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Utility;
using GameSettingsParser.ViewModels.Configuration.General;
using Newtonsoft.Json;

namespace GameSettingsParser.Model.Configuration.General
{
    public class GeneralConfigurationModel : IConfigurationModel
    {
        [JsonIgnore]
        public string DisplayName => ConfigurationSectionConstants.General;
        
        [JsonIgnore]
        public Type ViewModelType => typeof(GeneralConfigurationViewModel);
        
        [JsonIgnore]
        public string? Section => null;
        
        public bool AutoOpenLastParsingProfile { get; set; } = true;
    }
}