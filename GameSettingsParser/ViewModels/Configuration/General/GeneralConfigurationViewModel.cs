using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.General;
using GameSettingsParser.Utility;
using GameSettingsParser.Views.Configuration.General;

namespace GameSettingsParser.ViewModels.Configuration.General
{
    public class GeneralConfigurationViewModel : BindableBase, IConfigurationViewModel
    {
        public Type ViewType => typeof(GeneralConfigurationView);
        
        public string DisplayName => ThisConfiguration?.DisplayName ?? "General";
        
        public IConfigurationModel? Configuration { get; set; }
        public GeneralConfigurationModel? ThisConfiguration => Configuration as GeneralConfigurationModel;
        
        private bool _autoOpenLastParsingProfile;
        public bool AutoOpenLastParsingProfile
        {
            get => _autoOpenLastParsingProfile;
            set => SetProperty(ref _autoOpenLastParsingProfile, value);
        }

        public void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.AutoOpenLastParsingProfile = AutoOpenLastParsingProfile;
        }

        public void ResetChanges()
        {
            if (ThisConfiguration != null)
            {
                AutoOpenLastParsingProfile = ThisConfiguration.AutoOpenLastParsingProfile;
            }
            else
            {
                AutoOpenLastParsingProfile = false;
            }
        }


        public bool CheckForChanges()
        {
            if (ThisConfiguration == null)
                return false;
            
            return ThisConfiguration.AutoOpenLastParsingProfile != AutoOpenLastParsingProfile;
        }
    }
}