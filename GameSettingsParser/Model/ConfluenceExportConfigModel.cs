using GameSettingsParser.Services.Confluence;

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
        public ConfluencePage? Page { get; set; }
        public ConfluenceExportConfigMode Mode { get; set; } = ConfluenceExportConfigMode.Append;
        
    }
}