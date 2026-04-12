using System.Drawing;
using GameSettingsParser.Model;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Settings;
using Tesseract;
using Rect = System.Windows.Rect;

namespace GameSettingsParser.Services.ImageAnalysis
{
    public class TesseractImageAnalysisService : IImageAnalysisService
    {
        private struct DynamicMarkupInstance
        {
            public Rectangle Rectangle;
            public MarkupTypeModel Type;
        }
        
        public ImageAnalysisResultModel? Analyse(ParsingProfileModel parsingProfile, string[] imagePathsToAnalyse)
        {
            ImageAnalysisResultModel imageAnalysisResult = new ImageAnalysisResultModel();
        
            foreach (var markupType in parsingProfile.MarkupTypes)
            {
                if (markupType.IsSearchArea)
                    continue;

                if (markupType.IsDynamic)
                {
                    // First we process dynamic instances, this is because other markup types can be positioned relatively
                    ProcessDynamicMarkupInstances(imageAnalysisResult, parsingProfile, markupType, imagePathsToAnalyse);
                }
            }
        
            return imageAnalysisResult;
        }
        
        /// <summary>
        /// Processes markup instances that are marked as dynamic, using text recognition services where applicable.
        /// </summary>
        /// <param name="resultModel"></param>
        /// <param name="parsingProfile"></param>
        /// <param name="markupType"></param>
        /// <param name="imagePaths"></param>
        /// <returns>Dictionary of a given image path to the dynamic markup instance on that image</returns>
        /// <exception cref="Exception">Throws when search areas are incorrectly set up or validated</exception>
        private Dictionary<string, DynamicMarkupInstance> ProcessDynamicMarkupInstances(ImageAnalysisResultModel resultModel, ParsingProfileModel parsingProfile, MarkupTypeModel markupType, string[] imagePaths)
        {
            var dynamicMarkupInstances = new Dictionary<string, DynamicMarkupInstance>();
            
            // Calculate whether we are searching for instances within a search area or the whole image
            Rectangle? searchAreaRectangle = null;
            if (markupType.HasSearchArea)
            {
                MarkupTypeModel searchAreaMarkupType = parsingProfile.GetMarkupTypeByName(markupType.SearchArea);
                var allInstances = parsingProfile.ImageInstances.SelectMany(imageInstance =>
                    imageInstance.MarkupInstances.Where(instance => instance.Type == searchAreaMarkupType)).ToList();

                var instanceCount = allInstances.Count;
                if(instanceCount == 0)
                    throw new Exception($"No instances of search area found for markup type \"{markupType.Name}\": {markupType.SearchArea}");
                
                if (instanceCount > 1)
                    throw new Exception($"Multiple instances of search area found for markup type \"{markupType.Name}\": {markupType.SearchArea}");
                
                var firstInstance = allInstances.First();
                searchAreaRectangle = RectToRectangle(firstInstance.Rect);
            }
            
            List<Bitmap> targetBitmaps = new List<Bitmap>();
            
            // Gather all instances of regions marked dynamic, create cropped bitmaps for future comparison
            foreach (var imageInstance in parsingProfile.ImageInstances)
            {
                var markupInstances = imageInstance.MarkupInstances.Where(instance => instance.Type == markupType);
                    
                foreach (var markupInstance in markupInstances)
                {
                    Bitmap bitmap = new Bitmap(imageInstance.Image.Path);
                    var croppedImage = bitmap.Clone(RectToRectangle(markupInstance.Rect), bitmap.PixelFormat);
                    targetBitmaps.Add(croppedImage);
                }
            }

            int wordGapThreshold = UserSettings.Instance.WordGapThreshold;
            float minimumConfidence = UserSettings.Instance.MinimumDynamicComparisonConfidence;

            // Iterate over each image, gathering each word instance and testing it against the target/training images
            using (var engine = new TesseractEngine(@"./TesseractData", "eng", EngineMode.Default))
            {
                foreach (var imagePath in imagePaths)
                {
                    string? bestMatchText = null;
                    Rectangle? bestMatchRectangle = null;
                    double confidence = 0.0;
                    
                    var bitmap = new Bitmap(imagePath);
                    if (markupType.HasSearchArea)
                        bitmap = bitmap.Clone(searchAreaRectangle!.Value, bitmap.PixelFormat);

                    using (var img = PixConverter.ToPix(bitmap))
                    {
                        using (var page = engine.Process(img))
                        {
                            using (var iterator = page.GetIterator())
                            {
                                iterator.Begin();

                                do
                                {
                                    var currentWordSequence = new List<(string, Rectangle)>();
                                    var isHighestConfidenceWordSequence = false;
                                    
                                    do
                                    {
                                        if (!iterator.TryGetBoundingBox(PageIteratorLevel.Word, out var boundingBox))
                                            continue;

                                        var wordText = iterator.GetText(PageIteratorLevel.Word);

                                        var rectangle = RectToRectangle(boundingBox);
                                        var regionBitmap = bitmap.Clone(rectangle, bitmap.PixelFormat);

                                        var textComparisonService = ContainerLocator.Current.Resolve<ITextComparisonService>();

                                        foreach (var targetBitmap in targetBitmaps)
                                        {
                                            var newConfidence = textComparisonService.GetConfidenceInterval(targetBitmap, regionBitmap);

                                            if (newConfidence > confidence && newConfidence > minimumConfidence)
                                            {
                                                confidence = newConfidence;
                                                bestMatchRectangle = rectangle;
                                                bestMatchText = wordText;
                                                isHighestConfidenceWordSequence = true;
                                            }
                                        }

                                        // Check the word sequence hasn't ended, if it has, condense the last sequence if it's likely our target
                                        if (currentWordSequence.Count > 0)
                                        {
                                            var lastRect = currentWordSequence.Last().Item2;
                                            if (rectangle.X - (lastRect.X + lastRect.Width) > wordGapThreshold)
                                            {
                                                if (isHighestConfidenceWordSequence)
                                                {
                                                    CondenseWordSequence(currentWordSequence, out var condensedText, out var condensedRect);
                                                    bestMatchText = condensedText;
                                                    bestMatchRectangle = condensedRect;
                                                    isHighestConfidenceWordSequence = false;
                                                    currentWordSequence.Clear();
                                                }
                                            }
                                        }
                                        
                                        currentWordSequence.Add((wordText, rectangle));
                                    }
                                    while (iterator.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                                } 
                                while (iterator.Next(PageIteratorLevel.TextLine));
                            }
                        }
                    }

                    if (bestMatchText != null && bestMatchRectangle != null)
                    {
                        ImageAnalysisResultModel.Setting? setting = resultModel.Settings.Any(item => item.ScreenshotPath == imagePath) 
                            ? resultModel.Settings.First(item => item.ScreenshotPath == imagePath) 
                            : new ImageAnalysisResultModel.Setting();
                        
                        if(!setting.MarkupTypeToValues.ContainsKey(markupType.Name))
                            setting.MarkupTypeToValues.Add(markupType.Name, new List<string>());
                        
                        setting.MarkupTypeToValues[markupType.Name].Add(bestMatchText);
                        
                        dynamicMarkupInstances.Add(imagePath, new DynamicMarkupInstance() { Rectangle = bestMatchRectangle.Value, Type = markupType });
                    }
                }
            }

            return dynamicMarkupInstances;
        }

        private void CondenseWordSequence(List<(string, Rectangle)> wordSequence, out string text, out Rectangle boundingBox)
        {
            text = string.Join(" ", wordSequence.Select(word => word.Item1));
            var combinedBox = Rectangle.Empty;
            wordSequence.ForEach(pair => { combinedBox = Rectangle.Union(combinedBox, pair.Item2); });
            boundingBox = combinedBox;
        }

        private double GetAverageConfidence(IEnumerable<Bitmap> targetBitmaps, Bitmap testBitmap)
        {
            return 0;
        }
        
        
        // TODO: Fix all this marshalling into multiple Rect implementations, or at least make it implicit conversions via extensions
        private static Rectangle RectToRectangle(Rect rect)
        {
            return new Rectangle(
                Convert.ToInt32(rect.X),
                Convert.ToInt32(rect.Y),
                Convert.ToInt32(rect.Width),
                Convert.ToInt32(rect.Height));
        }
        
        private static Rectangle RectToRectangle(Tesseract.Rect rect)
        {
            return new Rectangle(
                Convert.ToInt32(rect.X1),
                Convert.ToInt32(rect.Y1),
                Convert.ToInt32(rect.Width),
                Convert.ToInt32(rect.Height));
        }

        private static Rect RectangleToRect(Rectangle rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}