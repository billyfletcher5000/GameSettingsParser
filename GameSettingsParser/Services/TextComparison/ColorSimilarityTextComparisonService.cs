using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace GameSettingsParser.Services.TextComparison
{
    public class ColorSimilarityTextComparisonService : ITextComparisonService
    {
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB)
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

            return correlation;
        }
    }
}