using System.Collections.ObjectModel;
using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.Services.Configuration
{
    public interface IConfigurationSource
    {
        public ObservableCollection<IConfigurationModel> Configurations { get; }
    }
}