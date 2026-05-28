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
        public bool HasChanges { get; set; }
        
        [JsonIgnore]
        public string DisplayName => "General";
        
        [JsonIgnore]
        public Type ViewModelType => typeof(GeneralConfigurationViewModel);
        
        [JsonIgnore]
        public string? Section => null;
        
        [JsonIgnore]
        public IEnumerable<Type> AnalysisExportServiceTypes { get; }
        
        [JsonIgnore]
        public IEnumerable<Type> TextComparisonServiceTypes { get; }
        
        private string? _analysisExportServiceId = null;
        public string? AnalysisExportServiceId
        {
            get => _analysisExportServiceId;
            set
            {
                if (_analysisExportServiceId == value) return;
                
                _analysisExportServiceId = value;
                AnalysisExportServiceIdChanged?.Invoke(_analysisExportServiceId);
            }
        }
        public event Action<string?>? AnalysisExportServiceIdChanged;
        
        private string? _textComparisonServiceId = null;
        public string? TextComparisonServiceId
        {
            get => _textComparisonServiceId;
            set
            {
                if (_textComparisonServiceId == value) return;
                
                _textComparisonServiceId = value;
                TextComparisonServiceIdChanged?.Invoke(_textComparisonServiceId);
            }
        }
        public event Action<string?>? TextComparisonServiceIdChanged; 

        public GeneralConfigurationModel()
        {
            AnalysisExportServiceTypes = SwitchableServiceHelper.GetSwitchableServiceImplementations<IAnalysisExportService>();
            TextComparisonServiceTypes = SwitchableServiceHelper.GetSwitchableServiceImplementations<ITextComparisonService>();
            
            AnalysisExportServiceId = SwitchableServiceHelper.GetDefaultSwitchableServiceId<IAnalysisExportService>();
            TextComparisonServiceId = SwitchableServiceHelper.GetDefaultSwitchableServiceId<ITextComparisonService>();
        }
        
        public void ApplyChanges()
        {
            throw new NotImplementedException();
        }

        public void ResetChanges()
        {
            throw new NotImplementedException();
        }
    }
}