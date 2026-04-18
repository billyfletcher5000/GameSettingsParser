namespace GameSettingsParser.Model
{
    public class ImageAnalysisResultModel
    {
        public class ProcessedImage
        {
            public Dictionary<MarkupTypeModel, List<string>> MarkupTypeToValues { get; set; } = new();
            public string ScreenshotPath { get; set; } = "";
        }
        
        public List<ProcessedImage> ProcessedImages { get; init; } = [];
    }
}