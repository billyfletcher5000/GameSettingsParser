using GameSettingsParser.Views.Configuration.General;

namespace GameSettingsParser.ViewModels.Configuration.General
{
    public class GeneralConfigurationViewModel : IConfigurationViewModel
    {
        public Type ViewType => typeof(GeneralConfigurationView);
        
    }
}