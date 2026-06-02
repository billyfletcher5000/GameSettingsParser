using GameSettingsParser.Constants;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Utility;
using GameSettingsParser.ViewModels.Configuration.General;
using GameSettingsParser.ViewModels.Configuration.Project;
using Newtonsoft.Json;

namespace GameSettingsParser.Model.Configuration.Project
{
    public class ProjectConfigurationModel : IConfigurationModel
    {
        [JsonIgnore]
        public string DisplayName => "Project Settings";
        
        [JsonIgnore]
        public Type ViewModelType => typeof(ProjectConfigurationViewModel);
        
        [JsonIgnore]
        public string? Section => ConfigurationSectionConstants.Project;
        
        [JsonIgnore]
        public IEnumerable<Type> AnalysisExportServiceTypes { get; }
        
        [JsonIgnore]
        public IEnumerable<Type> TextComparisonServiceTypes { get; }
        
        private string? _analysisExportServiceId;

        public string? AnalysisExportServiceId
        {
            get => _analysisExportServiceId;
            set
            {
                if (_analysisExportServiceId == value)
                    return;
                
                _analysisExportServiceId = value;
                OnAnalysisExportServiceIdChanged?.Invoke(value);
            }
        }
        public event Action<string?>? OnAnalysisExportServiceIdChanged;
        
        private string? _textComparisonServiceId;

        public string? TextComparisonServiceId
        {
            get => _textComparisonServiceId;
            set
            {
                if (_textComparisonServiceId == value)
                    return;
                
                _textComparisonServiceId = value;
                OnTextComparisonServiceIdChanged?.Invoke(value);
            }
        }
        public event Action<string?>? OnTextComparisonServiceIdChanged;

        public ProjectConfigurationModel()
        {
            AnalysisExportServiceTypes = SwitchableServiceHelper.GetSwitchableServiceImplementations<IAnalysisExportService>();
            TextComparisonServiceTypes = SwitchableServiceHelper.GetSwitchableServiceImplementations<ITextComparisonService>();
            
            AnalysisExportServiceId = SwitchableServiceHelper.GetDefaultSwitchableServiceId<IAnalysisExportService>();
            TextComparisonServiceId = SwitchableServiceHelper.GetDefaultSwitchableServiceId<ITextComparisonService>();
        }
    }
}