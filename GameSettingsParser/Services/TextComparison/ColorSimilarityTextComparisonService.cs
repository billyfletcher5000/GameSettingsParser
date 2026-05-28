using System.Drawing;
using GameSettingsParser.Attributes;
using GameSettingsParser.Model;
using GameSettingsParser.Model.Configuration;
using GameSettingsParser.Model.Configuration.TextComparison;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace GameSettingsParser.Services.TextComparison
{
    [SwitchableService(nameof(ColorSimilarityTextComparisonConfigurationModel), "Color Similarity")]
    public class ColorSimilarityTextComparisonService : ITextComparisonService
    {
        private ColorSimilarityTextComparisonConfigurationModel? _thisConfiguration;

        public IConfigurationModel? Configuration
        {
            get => ThisConfiguration;
            set => ThisConfiguration = value as ColorSimilarityTextComparisonConfigurationModel;
        }

        public ColorSimilarityTextComparisonConfigurationModel? ThisConfiguration
        {
            get => _thisConfiguration;
            set => _thisConfiguration = value;
        }

        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB, ParsingProfileModel parsingProfile)
        {
            var matA = imageA.ToMat();
            var matB = imageB.ToMat();

            int[] channels = [0, 1, 2];
            int[] histSize = [8, 8, 8];
            Rangef[] ranges = [new Rangef(0, 256), new Rangef(0, 256), new Rangef(0, 256)];

            var histA = new Mat();
            var histB = new Mat();

            Cv2.CalcHist([matA], channels, null, histA, 3, histSize, ranges);
            Cv2.CalcHist([matB], channels, null, histB, 3, histSize, ranges);

            Cv2.Normalize(histA, histA, 1.0, 0.0, NormTypes.L1);
            Cv2.Normalize(histB, histB, 1.0, 0.0, NormTypes.L1);

            var correlation = 1.0 - Cv2.CompareHist(histA, histB, HistCompMethods.Bhattacharyya);

            if (correlation < ThisConfiguration?.MinimumConfidence)
                return 0.0;

            return correlation;
        }
    }
}