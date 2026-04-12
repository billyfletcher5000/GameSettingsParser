namespace GameSettingsParser.Model
{
    public class ImageAnalysisResultModel
    {
        public class Setting
        {
            public Dictionary<string, List<string>> MarkupTypeToValues { get; set; } = new();
            public string ScreenshotPath { get; set; } = "";
        }
        
        public List<Setting> Settings { get; init; } = [];
    }
}