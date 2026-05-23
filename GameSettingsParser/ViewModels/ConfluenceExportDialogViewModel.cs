using System.Collections.ObjectModel;
using GameSettingsParser.Model.Atlassian;
using GameSettingsParser.Services.Confluence;

namespace GameSettingsParser.ViewModels
{
    public class ConfluenceExportDialogViewModel : BindableBase
    {
        public ObservableCollection<string> SiteOptions { get; }
        public bool CanSelectSiteOption => _activeDataRetrievalCount == 0 && SiteOptions.Count > 0;

        public string Site
        {
            get => _site;
            set
            {
                _site = value;
                Config.SiteId = _siteTitleToSiteId[value];
                _ = RetrieveSpaceOptionsAsync(_cancellationTokenSource);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsOkEnabled));
            }
        }
        
        public ObservableCollection<string> SpaceOptions { get; }
        public bool CanSelectSpaceOption => _activeDataRetrievalCount == 0 && SpaceOptions.Count > 0;

        public string Space
        {
            get => _space;
            set
            {
                _space = value;
                Config.SpaceId = _spaceTitleToSpaceId[value];
                _ = RetrievePageOptionsAsync(_cancellationTokenSource);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsOkEnabled));
            }
        }
        
        public ObservableCollection<string> PageOptions { get; }
        public bool CanSelectPageOption => _activeDataRetrievalCount == 0 && PageOptions.Count > 0;

        public string Page
        {
            get => _page;
            set
            {
                _page = value;
                if (_pageTitleToPage.TryGetValue(value, out var page))
                {
                    Config.Page = page;
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsOkEnabled));
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
        
        public bool IsOkEnabled => !string.IsNullOrEmpty(Site) && !string.IsNullOrEmpty(Space) && !string.IsNullOrEmpty(Page);

        private ConfluenceApiService _confluenceApiService;
        private string _accessToken;
        
        private CancellationToken _cancellationTokenSource = new();
        private string _site = string.Empty;
        private string _space = string.Empty;
        private string _page = string.Empty;
        private Dictionary<string, string> _siteTitleToSiteId = new();
        private Dictionary<string, string> _spaceTitleToSpaceId = new();
        private Dictionary<string, ConfluencePage> _pageTitleToPage = new();
        private int _activeDataRetrievalCount;
        
        public ConfluenceExportConfigModel Config { get; } = new();

        public ConfluenceExportDialogViewModel(ConfluenceApiService confluenceApiService, string accessToken)
        {
            _confluenceApiService = confluenceApiService;
            _accessToken = accessToken;
            
            SiteOptions = new ObservableCollection<string>();
            SpaceOptions = new ObservableCollection<string>();
            PageOptions = new ObservableCollection<string>();
            
            _ = RetrieveWebsiteOptionsAsync(_cancellationTokenSource);
        }

        public async Task RetrieveWebsiteOptionsAsync(CancellationToken cancellationToken = default)
        {
            _activeDataRetrievalCount++;
            
            _siteTitleToSiteId.Clear();
            SiteOptions.Clear();
            
            var sites = await _confluenceApiService.GetConfluenceAccessibleResourcesAsync(_accessToken, cancellationToken);
            foreach (var site in sites)
            {
                _siteTitleToSiteId.Add(site.Name, site.Id);
            }
            
            SiteOptions.AddRange(_siteTitleToSiteId.Keys);
            Site = sites.FirstOrDefault()?.Name ?? string.Empty;
            
            _activeDataRetrievalCount--;
        }

        private async Task RetrieveSpaceOptionsAsync(CancellationToken cancellationToken = default)
        {
            _activeDataRetrievalCount++;
            
            _spaceTitleToSpaceId.Clear();
            SpaceOptions.Clear();
            
            var spaces = await _confluenceApiService.GetSpacesAsync(_accessToken, Config.SiteId, cancellationToken);
            foreach (var space in spaces)
            {
                if(space.Name != null && space.Id != null)
                    _spaceTitleToSpaceId.Add(space.Name, space.Id);
            }
            
            SpaceOptions.AddRange(_spaceTitleToSpaceId.Keys);
            Space = spaces.FirstOrDefault()?.Name ?? string.Empty;

            _activeDataRetrievalCount--;
        }

        private async Task RetrievePageOptionsAsync(CancellationToken cancellationToken = default)
        {
            _activeDataRetrievalCount++;
            
            _pageTitleToPage.Clear();
            PageOptions.Clear();
            
            var pages = await _confluenceApiService.GetPagesFromSpaceAsync(_accessToken, Config.SiteId, Config.SpaceId, cancellationToken);
            foreach (var page in pages)
            {
                if (!string.IsNullOrEmpty(page.Title) && !string.IsNullOrEmpty(page.Id) && page.Version != null)
                {
                    _pageTitleToPage.Add(page.Title, page);
                }
            }
            
            PageOptions.AddRange(_pageTitleToPage.Keys);
            Page = pages.FirstOrDefault()?.Title ?? string.Empty;
            
            _activeDataRetrievalCount--;
        }
    }
}