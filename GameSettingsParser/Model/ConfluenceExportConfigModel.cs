namespace GameSettingsParser.Model
{
    public enum ConfluenceExportConfigMode
    {
        Append,
        Overwrite
    }
    
    public class ConfluenceExportConfigModel
    {
        public string SiteId { get; set; } = string.Empty;
        public string SpaceId { get; set; } = string.Empty;
        public string PageId { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public int PageVersion { get; set; } = 1;
        public ConfluenceExportConfigMode Mode { get; set; } = ConfluenceExportConfigMode.Append;
        
    }
}