using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using GameSettingsParser.Model;
using GameSettingsParser.Services.TextComparison;
using GameSettingsParser.Settings;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Path = System.IO.Path;
using Rect = System.Windows.Rect;
using Rectangle = System.Drawing.Rectangle;

namespace GameSettingsParser.Services.ImageAnalysis
{
    public class TesseractImageAnalysisService : IImageAnalysisService
    {
        private struct DynamicMarkupInstance
        {
            public Rectangle Rectangle;
            // ReSharper disable once NotAccessedField.Local
            public MarkupTypeModel Type;
        }

        private class DynamicMarkupInstanceSet : Dictionary<string, DynamicMarkupInstance>
        {
        }
        
        private readonly TesseractEngine _engine;

        public TesseractImageAnalysisService()
        {
            _engine = new TesseractEngine(@"./TesseractData", "eng", EngineMode.Default);
            
            // Disable debug output, it prints an "Empty Page!!" error when we test regions that don't have text in them,
            // which while 'correct' is also expected behaviour. It would be better if we could suppress this specific message
            // but it does not appear possible.
            _engine.SetVariable("debug_file", "NUL");
        }
        
        public ImageAnalysisResultModel Analyse(ParsingProfileModel parsingProfile, string[] imagePathsToAnalyse)
        {
            var imageAnalysisResult = new ImageAnalysisResultModel();
            
            var dynamicMarkupInstanceSets = new Dictionary<string, DynamicMarkupInstanceSet>();
            foreach (var markupType in parsingProfile.MarkupTypes.Where(type => type.IsDynamic))
            {
                if (markupType.IsDynamic)
                {
                    // First we process dynamic instances, this is because other markup types can be positioned relatively
                    var dynamicMarkupInstances = ProcessDynamicMarkupInstances(imageAnalysisResult, parsingProfile, markupType, imagePathsToAnalyse);
                    if(dynamicMarkupInstances.Count > 0)
                        dynamicMarkupInstanceSets.Add(markupType.Name, dynamicMarkupInstances);
                }
            }
            
            foreach (var markupType in parsingProfile.MarkupTypes.Where(type => !type.IsDynamic))
            {
                if (markupType.IsSearchArea)
                    continue;

                foreach (var imagePath in imagePathsToAnalyse)
                {
                    Rectangle rectangle;
                    if (markupType.IsPositionedRelativeToOther)
                    {
                        var relativeRectangle = GetFirstRelativeRectangle(markupType, parsingProfile);
                        Rectangle parentRectangle;
                        if (dynamicMarkupInstanceSets.TryGetValue(markupType.PositionedRelativeTo, out var value))
                        {
                            parentRectangle = value[imagePath].Rectangle;
                        }
                        else
                        {
                            // Not entirely sure there's a valid use case for this, why would we need a relatively positioned markup type that's not linked to a dynamic?
                            // TODO: Fix this nesting, the general logic here may be necessarily complicated but surely some of it is unnecessary
                            parentRectangle = GetFirstAbsoluteRectangle(parsingProfile.GetMarkupTypeByName(markupType.PositionedRelativeTo), parsingProfile);
                        }
                        
                        rectangle = OffsetRelativeRectangle(relativeRectangle, parentRectangle, markupType.RelativePositioningType);
                    }
                    else
                    {
                        rectangle = GetFirstAbsoluteRectangle(markupType, parsingProfile);
                    }
                    
                    Bitmap bitmap = new Bitmap(imagePath);
                    var croppedImage = bitmap.Clone(rectangle, bitmap.PixelFormat);

                    using (var img = PixConverter.ToPix(croppedImage))
                    {
                        using (var page = _engine.Process(img))
                        {
                            var text = page.GetText();
                            
                            var imageFilename = Path.GetFileName(imagePath);
                            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/debug_images/potential_matches_static/");
                            croppedImage.Save($"{AppDomain.CurrentDomain.BaseDirectory}/debug_images/potential_matches_static/{imageFilename}_{markupType.Name}.png", ImageFormat.Png);
                            
                            ImageAnalysisResultModel.Setting setting = GetOrCreateSetting(imageAnalysisResult, imagePath);
                    
                            if(!setting.MarkupTypeToValues.ContainsKey(markupType.Name))
                                setting.MarkupTypeToValues.Add(markupType.Name, new List<string>());
                    
                            setting.MarkupTypeToValues[markupType.Name].Add(SanitizeOCRText(text));
                        }
                    }
                    
                }
            }
        
            return imageAnalysisResult;
        }

        private static ImageAnalysisResultModel.Setting GetOrCreateSetting(ImageAnalysisResultModel imageAnalysisResult, string imagePath)
        {
            if(imageAnalysisResult.Settings.Any(item => item.ScreenshotPath == imagePath)) 
                return imageAnalysisResult.Settings.First(item => item.ScreenshotPath == imagePath);

            var setting = new ImageAnalysisResultModel.Setting() { ScreenshotPath = imagePath };
            imageAnalysisResult.Settings.Add(setting);
            
            return setting;
        }

        private Rectangle GetFirstAbsoluteRectangle(MarkupTypeModel markupType, ParsingProfileModel parsingProfile)
        {
            var imageInstance = parsingProfile.ImageInstances.First(instance => instance.MarkupInstances.Any(markupInstance => markupInstance.Type == markupType));
            var targetMarkupInstance = imageInstance.MarkupInstances.First(markupInstance => markupInstance.Type == markupType);
            return RectToRectangle(targetMarkupInstance.Rect);
        }

        private Rectangle GetFirstRelativeRectangle(MarkupTypeModel markupType, ParsingProfileModel parsingProfile)
        {
            if (!markupType.IsPositionedRelativeToOther)
                throw new Exception("Markup type is not positioned relatively to another markup type");
            
            var targetType = parsingProfile.GetMarkupTypeByName(markupType.PositionedRelativeTo);

            var imageInstance = parsingProfile.ImageInstances.First(instance => 
                instance.MarkupInstances.Any(markupInstance => markupInstance.Type == targetType)
                && instance.MarkupInstances.Any(markupInstance => markupInstance.Type == markupType));
            
            var targetMarkupInstance = imageInstance.MarkupInstances.First(markupInstance => markupInstance.Type == targetType);
            var relativeMarkupInstance = imageInstance.MarkupInstances.First(markupInstance => markupInstance.Type == markupType);
            
            var vec = relativeMarkupInstance.Rect.Location - targetMarkupInstance.Rect.Location;
            return new Rectangle(Convert.ToInt32(vec.X), Convert.ToInt32(vec.Y), Convert.ToInt32(relativeMarkupInstance.Rect.Width), Convert.ToInt32(relativeMarkupInstance.Rect.Height));
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
        private DynamicMarkupInstanceSet ProcessDynamicMarkupInstances(ImageAnalysisResultModel resultModel, ParsingProfileModel parsingProfile, MarkupTypeModel markupType, string[] imagePaths)
        {
            var dynamicMarkupInstances = new DynamicMarkupInstanceSet();
            
            // Calculate whether we are searching for instances within a search area or the whole image
            Rectangle? searchAreaRectangle = null;
            if (markupType.HasSearchArea)
            {
                var searchAreaMarkupType = parsingProfile.GetMarkupTypeByName(markupType.SearchArea);
                var allInstances = parsingProfile.ImageInstances.SelectMany(imageInstance =>
                    imageInstance.MarkupInstances.Where(instance => instance.Type == searchAreaMarkupType)).ToList();

                var instanceCount = allInstances.Count;
                if (instanceCount == 0)
                    throw new Exception($"No instances of search area found for markup type \"{markupType.Name}\": {markupType.SearchArea}");
                
                if (instanceCount > 1)
                    throw new Exception($"Multiple instances of search area found for markup type \"{markupType.Name}\": {markupType.SearchArea}");
                
                var firstInstance = allInstances.First();
                searchAreaRectangle = RectToRectangle(firstInstance.Rect);
            }
            
            var targetBitmaps = new List<Bitmap>();
            
            // Gather all instances of regions marked dynamic, create cropped bitmaps for future comparison
            foreach (var imageInstance in parsingProfile.ImageInstances)
            {
                var markupInstances = imageInstance.MarkupInstances.Where(instance => instance.Type == markupType);
                    
                foreach (var markupInstance in markupInstances)
                {
                    if (imageInstance.Image == null || !File.Exists(imageInstance.Image.Path))
                        continue;
                    
                    var bitmap = new Bitmap(imageInstance.Image.Path);
                    var croppedImage = bitmap.Clone(RectToRectangle(markupInstance.Rect), bitmap.PixelFormat);
                    targetBitmaps.Add(croppedImage);

                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/debug_images/cropped_targets/");
                    croppedImage.Save($"{AppDomain.CurrentDomain.BaseDirectory}/debug_images/cropped_targets/{markupType.Name}_{imageInstance.Image.Name}.png", ImageFormat.Png);
                }
            }

            var wordGapThreshold = parsingProfile.WordGapThreshold;
            var minimumConfidence = parsingProfile.MinimumDynamicComparisonConfidence;

            // Iterate over each image, gathering each word instance and testing it against the target/training images
            foreach (var imagePath in imagePaths)
            {
                string? bestMatchText = null;
                Rectangle? bestMatchRectangle = null;
                double confidence = 0.0;
                
                var bitmap = new Bitmap(imagePath);
                if (markupType.HasSearchArea)
                    bitmap = bitmap.Clone(searchAreaRectangle!.Value, bitmap.PixelFormat);
                
                var currentWordSequence = new List<(string, Rectangle)>();
                var isHighestConfidenceWordSequence = false;

                using (var img = PixConverter.ToPix(bitmap))
                {
                    using (var page = _engine.Process(img))
                    {
                        using (var iterator = page.GetIterator())
                        {
                            iterator.Begin();

                            do
                            {
                                do
                                {
                                    if (!iterator.TryGetBoundingBox(PageIteratorLevel.Word, out var boundingBox))
                                        continue;
                                    
                                    var lineText = iterator.GetText(PageIteratorLevel.TextLine);
                                    Console.WriteLine($"Line: {lineText}");
                                    
                                    var wordText = iterator.GetText(PageIteratorLevel.Word);

                                    var rectangle = RectToRectangle(boundingBox);
                                    var regionBitmap = bitmap.Clone(rectangle, bitmap.PixelFormat);
                                    string imageFilename = Path.GetFileName(imagePath);
                                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/debug_images/potential_matches_dynamic/");
                                    regionBitmap.Save($"{AppDomain.CurrentDomain.BaseDirectory}/debug_images/potential_matches_dynamic/{imageFilename}_{wordText}.png", ImageFormat.Png);

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
                                        
                                        Console.WriteLine($"Text: {wordText}, Confidence: {newConfidence}, Best Match: {bestMatchText}, Best Match Confidence: {confidence}");
                                    }

                                    // Check the word sequence hasn't ended, if it has, condense the last sequence if it's likely our target
                                    if (currentWordSequence.Count > 0)
                                    {
                                        var lastRect = currentWordSequence.Last().Item2;
                                        if (wordGapThreshold >= 0 && rectangle.X - (lastRect.X + lastRect.Width) > wordGapThreshold)
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
                                
                                if (isHighestConfidenceWordSequence)
                                {
                                    CondenseWordSequence(currentWordSequence, out var condensedText, out var condensedRect);
                                    bestMatchText = condensedText;
                                    bestMatchRectangle = condensedRect;
                                    isHighestConfidenceWordSequence = false;
                                }
                                
                                currentWordSequence.Clear();
                            } 
                            while (iterator.Next(PageIteratorLevel.TextLine));
                        }
                    }
                }

                if (bestMatchText != null && bestMatchRectangle != null)
                {
                    // TODO: Add method to GetOrCreate
                    var setting = GetOrCreateSetting(resultModel, imagePath);
                    
                    if(!setting.MarkupTypeToValues.ContainsKey(markupType.Name))
                        setting.MarkupTypeToValues.Add(markupType.Name, new List<string>());
                    
                    setting.MarkupTypeToValues[markupType.Name].Add(SanitizeOCRText(bestMatchText));
                    
                    if(searchAreaRectangle.HasValue)
                        bestMatchRectangle = bestMatchRectangle.Value with { X = bestMatchRectangle.Value.Location.X + searchAreaRectangle.Value.Location.X, Y = bestMatchRectangle.Value.Location.Y + searchAreaRectangle.Value.Location.Y };
                    
                    dynamicMarkupInstances.Add(imagePath, new DynamicMarkupInstance() { Rectangle = bestMatchRectangle.Value, Type = markupType });
                }
            }
            

            return dynamicMarkupInstances;
        }

        private void CondenseWordSequence(List<(string, Rectangle)> wordSequence, out string text, out Rectangle boundingBox)
        {
            text = string.Join(" ", wordSequence.Select(word => word.Item1));
            var firstItem = wordSequence.First();
            var combinedBox = firstItem.Item2;
            foreach (var wordRectPair in wordSequence)
            {
                if (wordRectPair == firstItem)
                    continue;
                
                combinedBox = Rectangle.Union(combinedBox, wordRectPair.Item2);
            }
            boundingBox = combinedBox;
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
        
        private static Rectangle OffsetRelativeRectangle(Rectangle relativeRectangle, Rectangle parentRectangle, RelativePositioningType positioningType)
        {
            var halfRelativeWidth = relativeRectangle.Width / 2;
            var halfRelativeHeight = relativeRectangle.Height / 2;
            var halfParentWidth = parentRectangle.Width / 2;
            var halfParentHeight = parentRectangle.Height / 2;

            //TODO: Rethink this logic, I think it makes assumptions about the relative rectangle's pivot point, but it also seems to work
            var output = relativeRectangle;

            output.X = positioningType switch
            {
                RelativePositioningType.TopLeft or RelativePositioningType.MiddleLeft or RelativePositioningType.BottomLeft 
                    => parentRectangle.X + relativeRectangle.X,
                
                RelativePositioningType.TopMiddle or RelativePositioningType.MiddleMiddle or RelativePositioningType.BottomMiddle 
                    => parentRectangle.X + halfParentWidth + relativeRectangle.X + halfRelativeWidth,
                
                RelativePositioningType.TopRight or RelativePositioningType.MiddleRight or RelativePositioningType.BottomRight 
                    => parentRectangle.X + parentRectangle.Width + relativeRectangle.X + relativeRectangle.Width,
                
                _ => output.X
            };

            output.Y = positioningType switch
            {
                RelativePositioningType.TopLeft or RelativePositioningType.TopMiddle or RelativePositioningType.TopRight
                    => parentRectangle.Y + relativeRectangle.Y,
                
                RelativePositioningType.MiddleLeft or RelativePositioningType.MiddleMiddle or RelativePositioningType.MiddleRight 
                    => parentRectangle.Y + halfParentHeight + relativeRectangle.Y - halfRelativeHeight,
                
                RelativePositioningType.BottomLeft or RelativePositioningType.BottomMiddle or RelativePositioningType.BottomRight 
                    => parentRectangle.Y + parentRectangle.Height + relativeRectangle.Y - relativeRectangle.Height,
                
                _ => output.Y
            };

            return output;
        }
        
        private static string SanitizeOCRText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replace any run of whitespace (spaces, tabs, newlines, etc) and unwanted punctuation with a single space and trim
            return Regex.Replace(input, @"\s+|[_<>\[\]]+", " ").Trim();
        }
    }
}