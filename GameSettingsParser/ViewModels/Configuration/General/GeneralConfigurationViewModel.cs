using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.General;
using GameSettingsParser.Views.Configuration.General;

namespace GameSettingsParser.ViewModels.Configuration.General
{
    public class GeneralConfigurationViewModel : ConfigurationViewModelBase
    {
        public override Type ViewType => typeof(GeneralConfigurationView);

        public override string DisplayName
        {
            get
            {
                return ThisConfiguration?.DisplayName ?? "General";
            }
        }

        public GeneralConfigurationModel? ThisConfiguration => Configuration as GeneralConfigurationModel;
        
        private bool _autoOpenLastParsingProfile;
        public bool AutoOpenLastParsingProfile
        {
            get => _autoOpenLastParsingProfile;
            set => SetProperty(ref _autoOpenLastParsingProfile, value);
        }

        protected override void OnConfigurationUpdated()
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

        public override void ApplyChanges()
        {
            if (ThisConfiguration == null)
                return;
            
            ThisConfiguration.AutoOpenLastParsingProfile = AutoOpenLastParsingProfile;
        }

        public override bool CheckForChanges()
        {
            if (ThisConfiguration == null)
                return false;
            
            return ThisConfiguration.AutoOpenLastParsingProfile != AutoOpenLastParsingProfile;
        }
    }
}