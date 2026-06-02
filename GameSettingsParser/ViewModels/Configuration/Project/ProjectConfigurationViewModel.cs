using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.Project;
using GameSettingsParser.Utility;
using GameSettingsParser.Views.Configuration.Project;

namespace GameSettingsParser.ViewModels.Configuration.Project
{
    public class ProjectConfigurationViewModel : BindableBase, IConfigurationViewModel
    {
        public string DisplayName => ThisConfiguration?.DisplayName ?? "Project";
        public Type ViewType => typeof(ProjectConfigurationView);
        public IConfigurationModel? Configuration { get; set; }
        public ProjectConfigurationModel? ThisConfiguration => Configuration as ProjectConfigurationModel;
        
        public List<string> AnalysisExportServiceTypes => _analysisExportServiceDisplayNameToType.Keys.ToList();
        public List<string> TextComparisonServiceTypes => _textComparisonServiceDisplayNameToType.Keys.ToList();
        
        private string? _selectedAnalysisExportServiceType;
        public string? SelectedAnalysisExportServiceType
        {
            get => _selectedAnalysisExportServiceType;
            set => SetProperty(ref _selectedAnalysisExportServiceType, value);
        }

        private string? _selectedTextComparisonServiceType;
        public string? SelectedTextComparisonServiceType
        {
            get => _selectedTextComparisonServiceType; 
            set => SetProperty(ref _selectedTextComparisonServiceType, value);
        }
        
        private readonly Dictionary<string, Type> _analysisExportServiceDisplayNameToType = new();
        private readonly Dictionary<string, Type> _textComparisonServiceDisplayNameToType = new();

        public void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.AnalysisExportServiceId = SelectedAnalysisExportServiceType != null ? SwitchableServiceHelper.GetSwitchableServiceId(_analysisExportServiceDisplayNameToType[SelectedAnalysisExportServiceType]) : null;
            ThisConfiguration.TextComparisonServiceId = SelectedTextComparisonServiceType != null ? SwitchableServiceHelper.GetSwitchableServiceId(_textComparisonServiceDisplayNameToType[SelectedTextComparisonServiceType]) : null;
        }

        public void Initialise()
        {
            if (ThisConfiguration != null)
            {
                _analysisExportServiceDisplayNameToType.Clear();
                _textComparisonServiceDisplayNameToType.Clear();

                foreach (var type in ThisConfiguration.AnalysisExportServiceTypes)
                {
                    var displayName = SwitchableServiceHelper.GetSwitchableServiceDisplayName(type);
                    if (displayName != null)
                        _analysisExportServiceDisplayNameToType.Add(displayName, type);
                }
                
                foreach (var type in ThisConfiguration.TextComparisonServiceTypes)
                {
                    var displayName = SwitchableServiceHelper.GetSwitchableServiceDisplayName(type);
                    if (displayName != null)
                        _textComparisonServiceDisplayNameToType.Add(displayName, type);
                }

                SelectedAnalysisExportServiceType = GetSelectedAnalysisExportServiceDisplayName();
                SelectedTextComparisonServiceType = GetSelectedTextComparisonServiceDisplayName();
            }
            else
            {
                SelectedAnalysisExportServiceType = null;
                SelectedTextComparisonServiceType = null;
            }
        }

        private string? GetSelectedAnalysisExportServiceDisplayName()
        {
            if(ThisConfiguration == null)
                return null;
            
            var selectedAnalysisExportType = ThisConfiguration.AnalysisExportServiceTypes.FirstOrDefault(type => SwitchableServiceHelper.GetSwitchableServiceId(type) == ThisConfiguration.AnalysisExportServiceId);
            return selectedAnalysisExportType != null ? SwitchableServiceHelper.GetSwitchableServiceDisplayName(selectedAnalysisExportType) : AnalysisExportServiceTypes.First();
        }

        private string? GetSelectedTextComparisonServiceDisplayName()
        {
            if(ThisConfiguration == null)
                return null;
            
            var selectedTextComparisonType = ThisConfiguration.TextComparisonServiceTypes.FirstOrDefault(type => SwitchableServiceHelper.GetSwitchableServiceId(type) == ThisConfiguration.TextComparisonServiceId);
            return selectedTextComparisonType != null ? SwitchableServiceHelper.GetSwitchableServiceDisplayName(selectedTextComparisonType) : TextComparisonServiceTypes.First();       
        }


        public bool CheckForChanges()
        {
            if (ThisConfiguration == null)
                return false;
            
            return GetSelectedAnalysisExportServiceDisplayName() != SelectedAnalysisExportServiceType 
                   || GetSelectedTextComparisonServiceDisplayName() != SelectedTextComparisonServiceType;
        }
    }
}