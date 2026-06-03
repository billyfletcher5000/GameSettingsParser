using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.ViewModels.Configuration
{
    public abstract class ConfigurationViewModelBase : BindableBase, IConfigurationTreeViewItem
    {
        public abstract string DisplayName { get; }
        public abstract Type ViewType { get; }
        
        private IConfigurationModel? _configuration;
        public IConfigurationModel? Configuration 
        {
            get => _configuration;
            set
            {
                if (SetProperty(ref _configuration, value))
                    OnConfigurationUpdated();
            } 
        }
        
        public abstract void ApplyChanges();

        public abstract bool CheckForChanges();
        
        protected virtual void OnConfigurationUpdated() { }

        protected ConfigurationViewModelBase()
        {
            PropertyChanged += (_, _) => OnConfigurationChanged?.Invoke();
        }
        
        public event Action? OnConfigurationChanged;
        
        protected void RaiseConfigurationChanged()
        {
            OnConfigurationChanged?.Invoke();
        }
    }
}