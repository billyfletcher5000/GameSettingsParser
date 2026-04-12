
using System.Drawing;

namespace GameSettingsParser.Services.TextComparison
{
    public interface ITextComparisonService
    {
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB);
    }
}