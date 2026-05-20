using System.Collections.ObjectModel;
using GameSettingsParser.Model;

namespace GameSettingsParser.ViewModels
{
    public class ConfluenceExportDialogViewModel : BindableBase
    {
        public ObservableCollection<string> SpaceOptions { get; }

        public string Space
        {
            get => _space;
            set
            {
                _space = value;
                Config.SpaceId = _spaceTitleToSpaceId[value];
                RaisePropertyChanged();
            }
        }
        
        public ObservableCollection<string> PageOptions { get; }

        public string Page
        {
            get => _page;
            set
            {
                _page = value;
                Config.PageId = _pageTitleToPageId[value];
                RaisePropertyChanged();
            }
        }
        
        public ConfluenceExportConfigMode Mode
        {
            get => Config.Mode;
            set
            {
                Config.Mode = value;
                RaisePropertyChanged();
            }
        }
        
        public IEnumerable<ConfluenceExportConfigMode> ModeOptions => Enum.GetValues<ConfluenceExportConfigMode>();

        private string _space = string.Empty;
        private string _page = string.Empty;
        private readonly Dictionary<string, string> _spaceTitleToSpaceId;
        private readonly Dictionary<string, string> _pageTitleToPageId;
        
        public ConfluenceExportConfigModel Config { get; } = new();

        public ConfluenceExportDialogViewModel(Dictionary<string, string> spaceTitleToSpaceId, Dictionary<string, string> pageTitleToPageId)
        {
            _spaceTitleToSpaceId = spaceTitleToSpaceId;
            _pageTitleToPageId = pageTitleToPageId;

            SpaceOptions = new ObservableCollection<string>(_spaceTitleToSpaceId.Keys);
            PageOptions = new ObservableCollection<string>(_pageTitleToPageId.Keys);

            Space = SpaceOptions.First();
            Page = PageOptions.First();
        }
    }
}