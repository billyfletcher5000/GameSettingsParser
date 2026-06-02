using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public interface IConfigurationViewModel : IConfigurationTreeViewItem
    {
        public Type ViewType { get; }
        
        public IConfigurationModel? Configuration { get; set; }
        
        // Initialise() is called after configuration is set during creation
        // TODO: Work out a more MVVM approach to this, should be done via property but that meant being a class rather
        //       than an interface. Could just be a class and thus force BindableBase on children really.
        public void Initialise();
    }
}