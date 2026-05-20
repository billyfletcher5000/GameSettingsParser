namespace GameSettingsParser.Model
{
    public enum ConfluenceExportConfigMode
    {
        Append,
        Overwrite
    }
    
    public class ConfluenceExportConfigModel
    {
        public string SpaceId { get; set; } = string.Empty;
        public string PageId { get; set; } = string.Empty;
        public ConfluenceExportConfigMode Mode { get; set; } = ConfluenceExportConfigMode.Append;
    }
}